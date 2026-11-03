using System.Collections.Generic;
using UnityEngine;

namespace KoiAI.VFX
{
    public sealed class GunTracerSystem : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField, Min(1)]
        private int _capacity = 64;

        [Header("Tracer")]
        [SerializeField, Min(0.01f)]
        private float _speed = 250f;

        [SerializeField, Min(0.01f)]
        private float _length = 0.65f;

        [SerializeField, Min(0.001f)]
        private float _minDistance = 0.1f;

        private struct Tracer
        {
            public LineRenderer Renderer;
            public LineRenderer Prefab;
            public Vector3 Start;
            public Vector3 Direction;
            public float Distance;
            public float Traveled;
            public bool Active;
        }

        private Tracer[] _tracers;
        private readonly Dictionary<LineRenderer, Stack<LineRenderer>> _inactiveRenderers = new();
        private int _nextIndex;

        private void Awake()
        {
            _tracers = new Tracer[_capacity];
        }

        public bool Play(Vector3 start, Vector3 end, LineRenderer tracerPrefab)
        {
            if (!isActiveAndEnabled || _tracers == null || !tracerPrefab)
            {
                return false;
            }

            if (tracerPrefab.transform == transform || tracerPrefab.GetComponent<GunTracerSystem>() != null)
            {
                Debug.LogError("Tracer prefab cannot contain the GunTracerSystem itself. Assign a LineRenderer-only visual prefab.", this);
                return false;
            }

            Vector3 delta = end - start;
            float distance = delta.magnitude;

            if (distance < _minDistance)
            {
                return false;
            }

            int index = FindFreeIndex();

            if (index < 0)
            {
                return false;
            }

            ref Tracer tracer = ref _tracers[index];

            if (tracer.Prefab != tracerPrefab)
            {
                ReturnRenderer(tracer.Prefab, tracer.Renderer);
                tracer.Renderer = AcquireRenderer(tracerPrefab);
                tracer.Prefab = tracerPrefab;
            }

            tracer.Start = start;
            tracer.Direction = delta / distance;
            tracer.Distance = distance;
            tracer.Traveled = 0f;
            tracer.Active = true;

            tracer.Renderer.SetPosition(0, start);
            tracer.Renderer.SetPosition(1, start);
            tracer.Renderer.enabled = true;

            return true;
        }

        private LineRenderer CreateRenderer(LineRenderer prefab)
        {
            LineRenderer line = Instantiate(prefab, transform);
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.enabled = false;
            return line;
        }

        private LineRenderer AcquireRenderer(LineRenderer prefab)
        {
            if (_inactiveRenderers.TryGetValue(prefab, out Stack<LineRenderer> renderers)
                && renderers.Count > 0)
            {
                return renderers.Pop();
            }

            return CreateRenderer(prefab);
        }

        private void ReturnRenderer(LineRenderer prefab, LineRenderer renderer)
        {
            if (!renderer)
            {
                return;
            }

            renderer.enabled = false;

            if (!_inactiveRenderers.TryGetValue(prefab, out Stack<LineRenderer> renderers))
            {
                renderers = new Stack<LineRenderer>();
                _inactiveRenderers.Add(prefab, renderers);
            }

            renderers.Push(renderer);
        }

        private int FindFreeIndex()
        {
            int count = _tracers.Length;

            for (int i = 0; i < count; i++)
            {
                int index = (_nextIndex + i) % count;

                if (_tracers[index].Active)
                {
                    continue;
                }

                _nextIndex = (index + 1) % count;
                return index;
            }

            return -1;
        }

        private void Update()
        {
            if (_tracers == null)
            {
                return;
            }

            float deltaDistance = _speed * Time.deltaTime;

            for (int i = 0; i < _tracers.Length; i++)
            {
                ref Tracer tracer = ref _tracers[i];

                if (!tracer.Active)
                {
                    continue;
                }

                if (tracer.Traveled >= tracer.Distance)
                {
                    Release(ref tracer);
                    continue;
                }

                tracer.Traveled = Mathf.Min(tracer.Traveled + deltaDistance, tracer.Distance);

                float tailDistance = Mathf.Max(0f, tracer.Traveled - _length);

                Vector3 head = tracer.Start + tracer.Direction * tracer.Traveled;

                Vector3 tail = tracer.Start + tracer.Direction * tailDistance;

                tracer.Renderer.SetPosition(0, tail);
                tracer.Renderer.SetPosition(1, head);
            }
        }

        private static void Release(ref Tracer tracer)
        {
            tracer.Active = false;
            tracer.Renderer.enabled = false;
        }

        private void OnDisable()
        {
            if (_tracers == null)
                return;

            for (int i = 0; i < _tracers.Length; i++)
            {
                ref Tracer tracer = ref _tracers[i];

                if (tracer.Renderer)
                {
                    Release(ref tracer);
                }
            }
        }
    }
}
