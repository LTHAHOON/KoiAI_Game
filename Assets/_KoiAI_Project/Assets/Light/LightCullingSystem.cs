using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.Rendering;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [DisallowMultipleComponent]
    public sealed class LightCullingSystem : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _targetCamera;

        [Header("Lights")]
        [SerializeField] private List<Light> _lights = new();

        [Header("Budget")]
        [SerializeField, Range(1, 256)]
        private int _maxActiveLights = 240;

        [Header("Distance Fade")]
        [SerializeField, Min(0f)]
        private float _fadeStartDistance = 30f;

        [SerializeField, Min(0f)]
        private float _fadeEndDistance = 40f;

        [SerializeField, Min(0.01f)]
        private float _fadeInDuration = 0.3f;

        [SerializeField, Min(0.01f)]
        private float _fadeOutDuration = 0.2f;

        [Header("CullingGroup")]
        [SerializeField, Min(0f)]
        private float _extraRange = 2f;

        [SerializeField]
        private bool _updateMovingLights;

        [SerializeField, Min(0.02f)]
        private float _evaluationInterval = 0.05f;

        [Header("Priority")]
        [SerializeField, Range(0f, 1f)]
        private float _hysteresis = 0.15f;

        [Header("Debug")]
        [SerializeField, ReadOnly]
        private int _activeCount;

        [SerializeField, ReadOnly]
        private int _visibleCount;

        [SerializeField, ReadOnly]
        private int _candidateCount;

        private sealed class Entry
        {
            public Light Light;
            public Transform Transform;

            public float OriginalIntensity;
            public float CurrentIntensity;
            public float TargetIntensity;
            public float Score;

            public bool DesiredEnabled;
            public bool Visible;
            public bool Selected;

            // Frustum 밖에서 돌아온 경우 Fade 생략
            public bool SnapOnActivation = true;

            // -1이면 비활성
            public int ActiveSlot = -1;
        }

        private sealed class ScoreComparer : IComparer<int>
        {
            public Entry[] Entries;

            public int Compare(int a, int b)
            {
                int result = Entries[b].Score.CompareTo(
                    Entries[a].Score);

                return result != 0 ? result : a.CompareTo(b);
            }
        }

        private Entry[] _entries;
        private BoundingSphere[] _spheres;
        private int[] _activeIndices;
        private int _activeLength;

        private CullingGroup _group;

        private readonly List<int> _candidates = new();
        private readonly Dictionary<Light, int> _indices = new();
        private readonly ScoreComparer _comparer = new();

        private bool _visibilityReady;
        private bool _visibilityDirty;

        private float _nextEvaluationTime;

        // -----------------------------------
        // Editor
        // -----------------------------------

        [Button("Collect Scene Lights")]
        private void CollectLights()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var scene = gameObject.scene;

            if (!scene.IsValid() || !scene.isLoaded)
                return;

            Undo.RecordObject(this, "Collect Lights");

            _lights.Clear();

            HashSet<Light> unique = new();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Light[] found =
                    root.GetComponentsInChildren<Light>(true);

                foreach (Light light in found)
                {
                    if (light.type != LightType.Point &&
                        light.type != LightType.Spot)
                        continue;

                    if (light.lightmapBakeType ==
                        LightmapBakeType.Baked)
                        continue;

                    if (unique.Add(light))
                        _lights.Add(light);
                }
            }

            EditorUtility.SetDirty(this);

            Debug.Log(
                $"Collected {_lights.Count} lights.", this);
#endif
        }

        [Button("Clear Lights")]
        private void ClearLights()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            Undo.RecordObject(this, "Clear Lights");
            _lights.Clear();
            EditorUtility.SetDirty(this);
#endif
        }

        // -----------------------------------
        // Initialization
        // -----------------------------------

        private void OnEnable()
        {
            if (_targetCamera == null)
                _targetCamera = Camera.main;

            Initialize();

            RenderPipelineManager.endCameraRendering +=
                OnEndCameraRendering;
        }

        private void Initialize()
        {
            HashSet<Light> unique = new();
            List<Entry> valid = new(_lights.Count);

            _indices.Clear();

            foreach (Light light in _lights)
            {
                if (light == null || !unique.Add(light))
                    continue;

                if (light.type != LightType.Point &&
                    light.type != LightType.Spot)
                    continue;

                Entry entry = new()
                {
                    Light = light,
                    Transform = light.transform,
                    OriginalIntensity = light.intensity,
                    DesiredEnabled = light.enabled
                };

                _indices.Add(light, valid.Count);
                valid.Add(entry);
            }

            _entries = valid.ToArray();

            int count = _entries.Length;

            _spheres = new BoundingSphere[count];
            _activeIndices = new int[count];

            _activeLength = 0;
            _activeCount = 0;

            _comparer.Entries = _entries;

            _candidates.Clear();

            if (_candidates.Capacity < count)
                _candidates.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                Entry entry = _entries[i];

                _spheres[i] = CreateSphere(entry.Light);

                // 최초 상태는 모두 비활성화.
                // 첫 카메라 렌더링 후 실제 결과 적용.
                entry.Light.intensity = 0f;
                entry.Light.enabled = false;
            }

            _group = new CullingGroup
            {
                targetCamera = _targetCamera,
                onStateChanged = OnCullingStateChanged
            };

            _group.SetBoundingSpheres(_spheres);
            _group.SetBoundingSphereCount(count);

            _visibilityReady = false;
            _visibilityDirty = false;
            _nextEvaluationTime = 0f;
        }

        private BoundingSphere CreateSphere(Light light)
        {
            return new BoundingSphere(
                light.transform.position,
                Mathf.Max(0f, light.range) + _extraRange
            );
        }

        private void OnCullingStateChanged(CullingGroupEvent evt)
        {
            // 렌더링 콜백 안에서 정렬하지 않고
            // 다음 Update에 반영한다.
            if (_visibilityReady)
                _visibilityDirty = true;
        }

        private void OnEndCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (camera != _targetCamera)
                return;

            if (!_visibilityReady)
            {
                _visibilityReady = true;
                _visibilityDirty = true;
            }
        }

        // -----------------------------------
        // Update
        // -----------------------------------

        private void Update()
        {
            if (_group == null || !_visibilityReady)
                return;

            if (_visibilityDirty ||
                Time.unscaledTime >= _nextEvaluationTime)
            {
                _visibilityDirty = false;

                _nextEvaluationTime =
                    Time.unscaledTime + _evaluationInterval;

                if (_updateMovingLights)
                    UpdateSpheres();

                EvaluateLights();
            }
        }

        private void LateUpdate()
        {
            if (_group == null || !_visibilityReady)
                return;

            UpdateFades(Time.deltaTime);
            FillAvailableSlots();

            _activeCount = _activeLength;
        }

        private void UpdateSpheres()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                Light light = _entries[i].Light;

                if (light != null)
                    _spheres[i] = CreateSphere(light);
            }
        }

        // -----------------------------------
        // Visibility + Distance
        // -----------------------------------

        private void EvaluateLights()
        {
            _candidates.Clear();
            _visibleCount = 0;

            Camera camera = _targetCamera;

            if (camera == null ||
                !camera.isActiveAndEnabled)
            {
                DisableAllImmediate();
                return;
            }

            Vector3 cameraPosition =
                camera.transform.position;

            float start = Mathf.Max(
                0f, _fadeStartDistance);

            float end = Mathf.Max(
                start + 0.01f, _fadeEndDistance);

            float endSqr = end * end;

            // 1. CullingGroup 가시성 검사
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];
                Light light = entry.Light;

                entry.Selected = false;

                if (light == null ||
                    !light.gameObject.activeInHierarchy ||
                    !entry.DesiredEnabled)
                {
                    entry.Visible = false;
                    entry.SnapOnActivation = true;
                    DeactivateImmediate(i);
                    continue;
                }

                bool visible = _group.IsVisible(i);

                if (!visible)
                {
                    // 화면 밖: Fade 없이 즉시 OFF
                    entry.Visible = false;
                    entry.SnapOnActivation = true;
                    DeactivateImmediate(i);
                    continue;
                }

                _visibleCount++;

                entry.Visible = true;

                Vector3 delta =
                    entry.Transform.position -
                    cameraPosition;

                float distanceSqr = delta.sqrMagnitude;

                // 거리 밖인 기존 라이트는 Fade Out 허용.
                // 후보에서는 제외.
                if (distanceSqr >= endSqr)
                {
                    entry.TargetIntensity = 0f;
                    continue;
                }

                float distance = Mathf.Sqrt(distanceSqr);

                float factor = CalculateDistanceFactor(
                    distance, start, end);

                entry.TargetIntensity =
                    entry.OriginalIntensity * factor;

                // 카메라 거리 기준 우선순위
                float distanceScore =
                    1f / (1f + distanceSqr);

                float intensityScore =
                    1f + Mathf.Sqrt(
                        Mathf.Max(0f, entry.OriginalIntensity))
                        * 0.25f;

                entry.Score =
                    distanceScore * intensityScore;

                if (entry.ActiveSlot >= 0)
                    entry.Score *= 1f + _hysteresis;

                _candidates.Add(i);
            }

            // 2. 가까운 라이트부터 정렬
            _candidates.Sort(_comparer);

            _candidateCount = _candidates.Count;

            int budget = Mathf.Min(
                _maxActiveLights,
                _candidates.Count);

            for (int i = 0; i < budget; i++)
                _entries[_candidates[i]].Selected = true;

            // 3. 예산 때문에 밀린 라이트 즉시 OFF
            // 거리 밖에 있는 기존 라이트는 Fade Out 유지
            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];

                if (entry.ActiveSlot < 0 ||
                    !entry.Visible)
                    continue;

                if (entry.Selected)
                    continue;

                if (entry.TargetIntensity > 0f)
                {
                    // Budget 탈락
                    DeactivateImmediate(i);
                }
                // TargetIntensity == 0 이라면
                // UpdateFades에서 서서히 끈다.
            }

            FillAvailableSlots();
        }

        private static float CalculateDistanceFactor(
            float distance, float start, float end)
        {
            if (distance <= start)
                return 1f;

            if (distance >= end)
                return 0f;

            float t = (distance - start) / (end - start);

            // SmoothStep
            t = t * t * (3f - 2f * t);

            return 1f - t;
        }

        // -----------------------------------
        // Fade
        // -----------------------------------

        private void UpdateFades(float deltaTime)
        {
            int i = 0;

            while (i < _activeLength)
            {
                int index = _activeIndices[i];
                Entry entry = _entries[index];

                Light light = entry.Light;

                if (light == null ||
                    !light.gameObject.activeInHierarchy ||
                    !entry.Visible ||
                    !entry.DesiredEnabled)
                {
                    DeactivateImmediate(index);
                    continue;
                }

                float target = entry.Selected
                    ? entry.TargetIntensity
                    : 0f;

                float duration =
                    target > entry.CurrentIntensity
                        ? _fadeInDuration
                        : _fadeOutDuration;

                float speed =
                    entry.OriginalIntensity /
                    Mathf.Max(0.01f, duration);

                entry.CurrentIntensity = Mathf.MoveTowards(
                    entry.CurrentIntensity,
                    target,
                    speed * deltaTime);

                if (light.intensity != entry.CurrentIntensity)
                    light.intensity = entry.CurrentIntensity;

                if (target <= 0f &&
                    entry.CurrentIntensity <= 0.0001f)
                {
                    DeactivateImmediate(index);
                    continue;
                }

                i++;
            }
        }

        private void FillAvailableSlots()
        {
            int available = Mathf.Max(
                0, _maxActiveLights - _activeLength);

            if (available <= 0)
                return;

            for (int i = 0; i < _candidates.Count; i++)
            {
                if (available <= 0)
                    break;

                int index = _candidates[i];
                Entry entry = _entries[index];

                if (!entry.Selected ||
                    entry.ActiveSlot >= 0 ||
                    !entry.Visible ||
                    !entry.DesiredEnabled)
                    continue;

                Light light = entry.Light;

                if (light == null ||
                    !light.gameObject.activeInHierarchy)
                    continue;

                // Frustum 밖에서 다시 들어온 라이트는
                // Fade 없이 현재 거리의 밝기로 즉시 복원.
                float initialIntensity =
                    entry.SnapOnActivation
                        ? entry.TargetIntensity
                        : 0f;

                entry.SnapOnActivation = false;
                entry.CurrentIntensity = initialIntensity;

                light.intensity = initialIntensity;
                light.enabled = true;

                entry.ActiveSlot = _activeLength;
                _activeIndices[_activeLength++] = index;

                available--;
            }
        }

        private void DeactivateImmediate(int index)
        {
            Entry entry = _entries[index];

            if (entry.ActiveSlot < 0)
                return;

            Light light = entry.Light;

            if (light != null)
            {
                light.intensity = 0f;
                light.enabled = false;
            }

            int slot = entry.ActiveSlot;
            int lastSlot = --_activeLength;

            if (slot != lastSlot)
            {
                int movedIndex = _activeIndices[lastSlot];

                _activeIndices[slot] = movedIndex;
                _entries[movedIndex].ActiveSlot = slot;
            }

            entry.ActiveSlot = -1;
            entry.CurrentIntensity = 0f;
        }

        private void DisableAllImmediate()
        {
            while (_activeLength > 0)
            {
                int index = _activeIndices[_activeLength - 1];
                DeactivateImmediate(index);
            }

            _activeCount = 0;
            _candidateCount = 0;
            _visibleCount = 0;
        }

        // -----------------------------------
        // Public API
        // -----------------------------------

        public void SetDesiredEnabled(
            Light light, bool enabled)
        {
            if (light == null ||
                !_indices.TryGetValue(light, out int index))
                return;

            Entry entry = _entries[index];

            entry.DesiredEnabled = enabled;

            if (!enabled)
            {
                entry.Selected = false;
                DeactivateImmediate(index);
            }

            _visibilityDirty = true;
        }

        public void SetDesiredIntensity(
            Light light, float intensity)
        {
            if (light == null ||
                !_indices.TryGetValue(light, out int index))
                return;

            _entries[index].OriginalIntensity =
                Mathf.Max(0f, intensity);

            _visibilityDirty = true;
        }

        public void SetCamera(Camera camera)
        {
            if (_targetCamera == camera)
                return;

            DisableAllImmediate();

            _targetCamera = camera;

            if (_group != null)
                _group.targetCamera = camera;

            _visibilityReady = false;
            _visibilityDirty = false;

            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].Visible = false;
                _entries[i].SnapOnActivation = true;
            }
        }

        // -----------------------------------
        // Cleanup
        // -----------------------------------

        private void RestoreLights()
        {
            if (_entries == null)
                return;

            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];
                Light light = entry.Light;

                if (light == null)
                    continue;

                light.intensity = entry.OriginalIntensity;
                light.enabled = entry.DesiredEnabled;
            }
        }

        private void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -=
                OnEndCameraRendering;

            if (_group != null)
            {
                _group.onStateChanged = null;
                _group.Dispose();
                _group = null;
            }

            RestoreLights();

            _visibilityReady = false;
            _activeLength = 0;
            _activeCount = 0;
            _visibleCount = 0;
            _candidateCount = 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _maxActiveLights =
                Mathf.Clamp(_maxActiveLights, 1, 256);

            _fadeStartDistance =
                Mathf.Max(0f, _fadeStartDistance);

            _fadeEndDistance = Mathf.Max(
                _fadeStartDistance + 0.01f,
                _fadeEndDistance);

            _fadeInDuration =
                Mathf.Max(0.01f, _fadeInDuration);

            _fadeOutDuration =
                Mathf.Max(0.01f, _fadeOutDuration);

            _evaluationInterval =
                Mathf.Max(0.02f, _evaluationInterval);
        }
#endif
    }
}
