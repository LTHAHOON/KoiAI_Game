using UnityEngine;
using System;
using R3;
using UnityEngine.InputSystem;

namespace KoiAI.UI
{
    using KoiAI.Input;
    using KoiAI.Interact;

    public class MonitorHackingController : MonoBehaviour
    {
        [SerializeField]
        private MonitorInteractable _monitorInteract;
        [SerializeField]
        private MonitorHackingView _monitorHackingView;
        [SerializeField]
        private float _holdDelayHackingBar = 0.5f;

        private float _curHoldHackingBarAxis = 0f;
        private float _curHoldDelayTime = 0f;
        private IDisposable _interactDisposable;
        private bool _isHolding;

        private void Awake()
        {
            _interactDisposable = _monitorInteract.OnInteract.Subscribe(context =>
            {
                InputService.PlayerIA.Monitor.Hacking_Move.started += OnStartedHackingMoveBar;
                InputService.PlayerIA.Monitor.Hacking_Move.performed += OnHoldHackingMoveBar;
                InputService.PlayerIA.Monitor.Hacking_Move.canceled += OnCancelHackingMoveBar;
            }).AddTo(this);
        }

        private void Update()
        {
            if (_isHolding)
            {
                if (_curHoldDelayTime < _holdDelayHackingBar)
                {
                    _curHoldDelayTime += Time.unscaledDeltaTime;
                    return;
                }
                MoveHackingMoveBar(_curHoldHackingBarAxis);
                _curHoldDelayTime = 0f;
            }
        }

        private void MoveHackingMoveBar(float axis)
        {
            //오른쪽으로 바 이동
            if (axis > 0)
            {
                _monitorHackingView.MoveHackingBar(false);
            }
            //왼쪽으로 바 이동
            else if (axis < 0)
            {
                _monitorHackingView.MoveHackingBar(true);
            }
        }

        private void OnStartedHackingMoveBar(InputAction.CallbackContext context)
        {
            float axis = context.ReadValue<float>();
            MoveHackingMoveBar(axis);
        }

        private void OnHoldHackingMoveBar(InputAction.CallbackContext context)
        {
            _curHoldHackingBarAxis = context.ReadValue<float>();
            _isHolding = true;
            MoveHackingMoveBar(_curHoldHackingBarAxis);
        }

        private void OnCancelHackingMoveBar(InputAction.CallbackContext context)
        {
            _isHolding = false;
            _curHoldDelayTime = 0f;
            _curHoldHackingBarAxis = 0f;
        }

        private void OnDestroy()
        {
            InputService.PlayerIA.Monitor.Hacking_Move.started -= OnStartedHackingMoveBar;
            InputService.PlayerIA.Monitor.Hacking_Move.performed -= OnHoldHackingMoveBar;
            InputService.PlayerIA.Monitor.Hacking_Move.canceled -= OnCancelHackingMoveBar;
        }
    }
}
