using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Input
{
    public enum InputActionMapContext
    {
        Global,
        Player,
        Lobbdy
    }

    [RequireComponent(typeof(PlayerInput))]
    public class InputService : MonoBehaviour
    {
        private static PlayerInput _playerInput;

        private static PlayerInputAction _playerIA;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _playerInput = GetComponent<PlayerInput>();
            _playerIA = new();
            _playerInput.actions = _playerIA.asset;
        }

        public static void SwitchCurrentActionMap(InputActionMapContext actionMap)
        {
            _playerInput.SwitchCurrentActionMap(actionMap.ToString());
            _playerIA.Global.Enable();
        }

        public static void SetEnableActionMap(InputActionMapContext actionMap)
        {
            switch (actionMap)
            {
                case InputActionMapContext.Global:
                    _playerInput.SwitchCurrentActionMap(actionMap.ToString());
                    return;
                case InputActionMapContext.Player:
                    _playerIA.Lobby.Disable();
                    _playerIA.Player.Enable();
                    Debug.Log("asd");
                    break;
                case InputActionMapContext.Lobbdy:
                    _playerIA.Player.Disable();
                    _playerIA.Lobby.Enable();
                    break;
            }
            _playerIA.Global.Enable();
        }

        /// <summary>
        /// playerIA 재연결
        /// </summary>
        public static void ReconnectInputAction()
        {
            _playerIA.Disable();
            _playerIA.Enable();
        }

        public static PlayerInputAction PlayerIA => _playerIA;

    }
}
