
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI
{
    public sealed class CameraOcclusionSystem : MonoBehaviour
    {
        [SerializeField]
        private LayerMask _targetMask;
        [SerializeField]
        private string _ditherAlphaProperty = "_Dither_Alpha";
        [Header("Detection")]
        [SerializeField, Min(1)]
        private int _maxDitheringObject = 64;
        [SerializeField, Min(0.01f)]
        private float _detectRadius = 3f;
        [SerializeField, Min(0f)]
        private float _detectInterval = 0.05f;
        [SerializeField, Range(0f, 1f)]
        private float _minOccludedAlpha = 0.2f;

        private Camera _camera;
        private Collider[] _colliders;
        private int _alphaPropertyId;
        private Dictionary<ulong, DitherableObject> _dicDitherableObject = new();
        private HashSet<DitherableObject> _occludedObjects = new();
        private HashSet<DitherableObject> _detectedObjects = new();
        private CancellationTokenSource _detectLoopCts;

        private void Awake()
        {
            _camera = Camera.main;
            _colliders = new Collider[Mathf.Max(1, _maxDitheringObject)];
            _alphaPropertyId = Shader.PropertyToID(_ditherAlphaProperty);
        }

        private void OnEnable()
        {
            if (!_camera || _colliders == null)
            {
                return;
            }

            _detectLoopCts = new CancellationTokenSource();
            RunDetection(_detectLoopCts.Token).Forget();
        }


        private void OnDisable()
        {
            _detectLoopCts?.Cancel();
            _detectLoopCts?.Dispose();
            _detectLoopCts = null;

            foreach (DitherableObject ditherableObject in _occludedObjects)
            {
                if (ditherableObject)
                {
                    ditherableObject.SetOccluded(1f);
                }
            }

            _occludedObjects.Clear();
            _detectedObjects.Clear();
        }

        private void DetectObjects()
        {
            _detectedObjects.Clear();

            Vector3 cameraPosition = _camera.transform.position;
            int detectedCount = Physics.OverlapSphereNonAlloc(
                cameraPosition,
                _detectRadius,
                _colliders,
                _targetMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < detectedCount; i++)
            {
                Collider collider = _colliders[i];
                if (!collider)
                {
                    continue;
                }

                ulong colliderId = collider.GetEntityULongID();
                if (!_dicDitherableObject.TryGetValue(colliderId, out DitherableObject ditherableObject))
                {
                    DitherableObject newDitherableObject = collider.GetComponentInParent<DitherableObject>();
                    _dicDitherableObject.Add(colliderId, newDitherableObject);
                    ditherableObject = newDitherableObject;
                }
                if (!ditherableObject)
                {
                    continue;
                }

                ditherableObject.Initialize(_alphaPropertyId);
                if (_detectedObjects.Add(ditherableObject))
                {
                    float distance = Vector3.Distance(cameraPosition, collider.ClosestPoint(cameraPosition));
                    float distanceRatio = Mathf.Clamp01(distance / _detectRadius);
                    float targetAlpha = Mathf.Lerp(_minOccludedAlpha, 1f, distanceRatio);
                    ditherableObject.SetOccluded(targetAlpha);
                }
            }

            foreach (DitherableObject ditherableObject in _occludedObjects)
            {
                if (!_detectedObjects.Contains(ditherableObject) && ditherableObject)
                {
                    ditherableObject.SetOccluded(1f);
                }
            }

            (_detectedObjects, _occludedObjects) = (_occludedObjects, _detectedObjects);
        }

        private async UniTask RunDetection(CancellationToken token)
        {
            try
            {
                float nextDetectTime = 0f;

                while (true)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);

                    if (Time.time < nextDetectTime)
                    {
                        continue;
                    }

                    DetectObjects();

                    nextDetectTime = Time.time + _detectInterval;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
        }
    }
}