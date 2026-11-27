using KoiAI.Item;
using KoiAI.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.KoiCamera
{
    public enum PlayerCameraState
    {
        Gameplay,
        Aim
    }

    /// <summary>
    /// Selects the player camera by changing Cinemachine priority. Blend timing is configured on CinemachineBrain.
    /// </summary>
    public sealed class PlayerCameraStateController : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CinemachineCamera _gameplayCamera;
        [SerializeField] private CinemachineCamera _aimCamera;
        [SerializeField] private int _gameplayPriority = 10;
        [SerializeField] private int _aimPriority;

        private WeaponControllerBase _weaponController;
        private PlayerCameraState _currentState = PlayerCameraState.Gameplay;

        public PlayerCameraState CurrentState => _currentState;

        private void Awake()
        {
            if (!_playerController)
            {
                _playerController = GetComponent<PlayerController>();
            }
        }

        private void OnEnable()
        {
            if (_playerController)
            {
                _playerController.WeaponControllerChanged += OnWeaponControllerChanged;
                OnWeaponControllerChanged(_playerController.CurrentWeaponController);
            }

            ResetState();
        }

        private void OnDisable()
        {
            UnsubscribeWeaponController();
            if (_playerController)
            {
                _playerController.WeaponControllerChanged -= OnWeaponControllerChanged;
            }
        }

        public void ChangeState(PlayerCameraState state)
        {
            if (_currentState == state)
            {
                return;
            }

            _currentState = state;
            ApplyState(state);
        }

        public void ResetState()
        {
            _currentState = PlayerCameraState.Gameplay;
            ApplyState(_currentState);
        }

        private void ApplyState(PlayerCameraState state)
        {
            if (_gameplayCamera)
            {
                _gameplayCamera.Priority = state == PlayerCameraState.Gameplay
                    ? _gameplayPriority
                    : _aimPriority;
            }

            if (_aimCamera)
            {
                _aimCamera.Priority = state == PlayerCameraState.Aim
                    ? _gameplayPriority
                    : _aimPriority;
            }
        }

        private void OnWeaponControllerChanged(WeaponControllerBase weaponController)
        {
            UnsubscribeWeaponController();
            _weaponController = weaponController;
            if (!_weaponController)
            {
                ResetState();
                return;
            }

            _weaponController.AimStateChanged += OnAimStateChanged;
            OnAimStateChanged(_weaponController.IsAiming());
        }

        private void UnsubscribeWeaponController()
        {
            if (_weaponController)
            {
                _weaponController.AimStateChanged -= OnAimStateChanged;
                _weaponController = null;
            }
        }

        private void OnAimStateChanged(bool isAiming)
        {
            ChangeState(isAiming ? PlayerCameraState.Aim : PlayerCameraState.Gameplay);
        }
    }
}
