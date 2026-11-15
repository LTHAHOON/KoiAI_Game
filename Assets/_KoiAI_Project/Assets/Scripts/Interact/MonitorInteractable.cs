using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.Input;
    using KoiAI.Player;
    using KoiAI.UI;
    using UnityEngine.InputSystem;

    public struct MonitorInteractContext
    {
        public int Password { get; set; }
    }

    public class MonitorInteractable : BaseInteractable<MonitorInteractContext>, IPlayerInteractable
    {
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
            if (dataEvent.Password != dataEvent.Password)
            {
                return;
            }
            Debug.Log("MonitorInteractable: Interact with correct password.");
            base.Interact(dataEvent);
        }


    }
}
