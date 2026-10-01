using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    using KoiAI.Input;
    public interface IPlayerInteractable
    {
        void Interact(PlayerInteractor playerInteractor);
    }

    public class PlayerInteractor : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [SerializeField]
        private Transform _rayOrigin;
        [SerializeField]
        [Min(0.01f)]
        private float _interactRadius = 0.2f;
        [SerializeField]
        [Min(0f)]
        private float _interactDistance = 3f;
        [SerializeField]
        private LayerMask _interactableLayerMask = ~0;
        [SerializeField]
        [Min(1)]
        private int _maxHitCount = 16;
        [SerializeField]
        private string _playerInteractableTag = "PlayerInteractable";

        private RaycastHit[] _sphereCastHits;
        private readonly List<MonoBehaviour> _parentBehaviours = new();
        private Transform _cachedRayOrigin;
        private bool _isInputSubscribed;

        private void Awake()
        {
            int hitCount = Mathf.Max(1, _maxHitCount);
            _sphereCastHits = new RaycastHit[hitCount];
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void Start()
        {
            // InputService가 먼저 초기화되지 않은 경우를 대비해 한 번 더 연결을 시도합니다.
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
        }

        #region Input

        private void SubscribeInput()
        {
            if (_isInputSubscribed || InputService.PlayerIA == null)
            {
                return;
            }

            InputService.PlayerIA.Player.Interact.performed += OnInteract;
            _isInputSubscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!_isInputSubscribed || InputService.PlayerIA == null)
            {
                return;
            }

            InputService.PlayerIA.Player.Interact.performed -= OnInteract;
            _isInputSubscribed = false;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (!TryGetInteractable(out IPlayerInteractable interactable))
            {
                return;
            }

            interactable.Interact(this);
        }

        #endregion

        #region Interactable Search

        public bool TryGetInteractable(out IPlayerInteractable interactable)
        {
            interactable = null;
            if (!TryGetNearestHit(out RaycastHit hit))
            {
                return false;
            }

            // 콜라이더가 자식 오브젝트에 있어도 부모의 상호작용 컴포넌트를 찾습니다.
            _parentBehaviours.Clear();
            if (hit.collider.TryGetComponent(out IPlayerInteractable playerInteractable))
            {
                interactable = playerInteractable;
                return true;
            }

            return false;
        }

        public bool TryGetInteractable<T>(out T interactable) where T : Component
        {
            interactable = null;
            if (!TryGetNearestHit(out RaycastHit hit))
            {
                return false;
            }

            interactable = hit.collider.GetComponentInParent<T>();
            return interactable != null;
        }

        private bool TryGetNearestHit(out RaycastHit hit)
        {
            hit = default;
            Transform rayOrigin = GetRayOrigin();
            if (rayOrigin == null || _interactDistance <= 0f)
            {
                return false;
            }

            int hitCount = Physics.SphereCastNonAlloc(
                rayOrigin.position,
                _interactRadius,
                rayOrigin.forward,
                _sphereCastHits,
                _interactDistance,
                _interactableLayerMask,
                QueryTriggerInteraction.Ignore);

            float nearestDistance = float.PositiveInfinity;
            bool isFound = false;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit candidateHit = _sphereCastHits[i];
                if (candidateHit.collider == null || !candidateHit.collider.CompareTag(_playerInteractableTag))
                {
                    continue;
                }

                if (candidateHit.distance >= nearestDistance)
                {
                    continue;
                }

                hit = candidateHit;
                nearestDistance = candidateHit.distance;
                isFound = true;
            }

            return isFound;
        }

        private Transform GetRayOrigin()
        {
            if (_rayOrigin != null)
            {
                return _rayOrigin;
            }

            if (_cachedRayOrigin == null && Camera.main != null)
            {
                _cachedRayOrigin = Camera.main.transform;
            }

            return _cachedRayOrigin;
        }

        #endregion
    }
}
