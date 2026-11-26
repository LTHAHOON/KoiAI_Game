using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace KoiAI.Interact
{
    using KoiAI.Input;
    using KoiAI.Player;
    using KoiAI.Rendering;
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
        private MonitorScreenWindow _monitorScreenView;

        private bool _isOn = false;

        public void Interact(PlayerInteractor playerInteractor)
        {
            if (_isOn)
            {
                return;
            }

            DetectMaskRendererFeature.SetInteractOutlineAlpha(0f);
            _isOn = true;
            _monitorScreenView.OnInteractToCheckPin += Interact;
            _monitorScreenView.Show();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            InputService.SetEnableActionMap(InputActionMapContext.Monitor);
            InputService.PlayerIA.Monitor.Monitor_Cancel.performed += CloseMonitorInteraction;
            _monitorScreenView.BindCloseButton(CloseMonitorScreen);
        }
        
        public void OnCompletedSetOutlineRenderer()
        {
            DetectMaskRendererFeature.SetInteractOutlineAlpha(1f);
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

            DetectMaskRendererFeature.SetInteractOutlineAlpha(1f);
            _isOn = false;
            _monitorScreenView.OnInteractToCheckPin -= Interact;
            _monitorScreenView.Hide();
            InputService.PlayerIA.Monitor.SetCallbacks(null);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputService.SetEnableActionMap(InputActionMapContext.Player);
        }


        public override void Interact(MonitorInteractContext dataEvent)
        {
            bool canLogin = dataEvent.PinCode == _monitorUserInfo.PinCode;
            _monitorScreenView?.OnGetResultPinLogin?.Invoke(canLogin);
            if (!canLogin)
            {
                Debug.Log("PinCode가 정확하지않습니다.");
                return;
            }

            _monitorScreenView.OpenChildView<MonitorHackingWindow>();
            Debug.Log("PC 접속하였습니다.");
            base.Interact(dataEvent);
        }

    }
}
