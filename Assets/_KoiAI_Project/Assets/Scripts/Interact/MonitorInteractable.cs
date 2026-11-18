using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace KoiAI.Interact
{
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
        [SerializeField]
        private MonitorScreenView _monitorScreenView;
        private bool _isOn = false;

        public void Interact(PlayerInteractor playerInteractor)
        {
            if (_isOn)
            {
                return;
            }

            _isOn = true;
            _monitorScreenView.OnInteractToCheckPin = Interact;
            _monitorScreenView.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            InputService.SetEnableActionMap(InputActionMapContext.Monitor);
            InputService.PlayerIA.Monitor.Monitor_Cancel.performed += CloseMonitorInteraction;
            _monitorScreenView.BindCloseButton(CloseMonitorScreen);
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            InputService.SetEnableActionMap(InputActionMapContext.Player);
        }

        public override void Interact(MonitorInteractContext dataEvent)
        {
            if (dataEvent.PinCode != _monitorUserInfo.PinCode)
            {
                Debug.Log("PinCode가 정확하지않습니다.");
                return;
            }
            _monitorScreenView.OpenChildView<MonitorHackingView>();
            Debug.Log("PC 접속하였습니다.");
            base.Interact(dataEvent);
        }


    }
}
