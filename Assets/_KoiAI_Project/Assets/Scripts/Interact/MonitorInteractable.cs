using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Interact
{
    using System;
    using KoiAI.Input;
    using KoiAI.Player;
    using KoiAI.UI;


    [Serializable]
    public struct MonitorInteractContext
    {
        [SerializeField]
        private int _pinCode;

        public MonitorInteractContext(int pinCode)
        {
            _pinCode = pinCode;
        }

        public readonly int PinCode => _pinCode;
    }

    public class MonitorInteractable : BaseInteractable<MonitorInteractContext>, IPlayerInteractable
    {
        [SerializeField]
        private MonitorInteractContext _monitorUserInfo;

        private MonitorScreenView _monitorScreenView;
        private bool _isOn = false;

        private void Awake()
        {
            _monitorScreenView = FindAnyObjectByType<MonitorScreenView>(FindObjectsInactive.Include);
            if (_monitorScreenView == null)
            {
                Debug.LogError("MonitorScreenView를 찾을 수 없습니다. MonitorInteractable이 올바르게 작동하지 않을 수 있습니다.");
            }
        }

        public void Interact(PlayerInteractor playerInteractor)
        {
            if (_isOn)
            {
                return;
            }

            _isOn = true;
            _monitorScreenView.OnInteractToCheckPin = Interact;
            _monitorScreenView.Show();
            InputService.SetEnableActionMap(InputActionMapContext.Monitor);
            InputService.PlayerIA.Monitor.Monitor_Cancel.performed += CloseMonitorInteraction;
            _monitorScreenView.BindCloseButton(OnCloseMonitorScreen);
        }

        private void OnCloseMonitorScreen()
        {
            CloseMonitorScreen();
        }
        
        private void CloseMonitorInteraction(InputAction.CallbackContext context)
        {
            if (context.performed && _isOn)
            {
                CloseMonitorScreen();
            }
        }

        private void CloseMonitorScreen()
        {
            if (!_isOn)
            {
                return;
            }

            _isOn = false;
            _monitorScreenView.Hide();
            InputService.PlayerIA.Monitor.Monitor_Cancel.performed -= CloseMonitorInteraction;
            InputService.SetEnableActionMap(InputActionMapContext.Player);
        }

        public override void Interact(MonitorInteractContext dataEvent)
        {
            if (dataEvent.PinCode != _monitorUserInfo.PinCode)
            {
                Debug.Log("PinCode가 정확하지않습니다.");
                return;
            }
            Debug.Log("PC 접속하였습니다.");
            base.Interact(dataEvent);
        }


    }
}
