using KoiAI.A_Star;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    public class PlayerWayPoint : PlayerFeature
    {
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private WayPointHandle _playerWayPointControl;
        [SerializeField]
        private float _buildDelayTime = 2f;

        private float _curTime = 0;
        private bool _isAutoBuild = false;
        private bool _isShowedWayPoint = false;
        public override void Init(PlayerInputAction playerIA)
        {
            playerIA.Player.SetVisibleWayPoint.performed += OnSetVisibleWayPoint;
            playerIA.Player.Move.performed += OnRebuildWayPoint;
            playerIA.Player.Move.canceled += OnRebuildWayPoint;
            _playerWayPointControl.InitStartAndGoal(transform.position, _target.localPosition);
        }

        public void OnSetVisibleWayPoint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _isShowedWayPoint = !_isShowedWayPoint;
                if (_playerWayPointControl.IsBuilding)
                {
                    return;
                }
                _playerWayPointControl.InitStartAndGoal(transform.position, _target.position);
                _playerWayPointControl.BuildOrClearWayPoint(_isShowedWayPoint);
            }
        }

        public void OnRebuildWayPoint(InputAction.CallbackContext context)
        {
            if (_isShowedWayPoint == false || _playerWayPointControl.IsBuilding)
            {
                _isAutoBuild = false;
                return;
            }
            if (context.performed)
            {
                _isAutoBuild = true;
            }
            if (context.canceled)
            {
                _isAutoBuild = false;
                _playerWayPointControl.InitStartAndGoal(transform.position, _target.position);
                _playerWayPointControl.BuildWayPoint();
            }
        }
    
        public override void UpdateFeature()
        {
            if (_isShowedWayPoint == false || _playerWayPointControl.IsBuilding)
            {
                return;
            }
            if(_isAutoBuild)
            {
                if(_curTime < _buildDelayTime)
                {
                    _curTime += Time.deltaTime;
                    return;
                }
                _curTime = 0f;
                _playerWayPointControl.InitStartAndGoal(transform.position, _target.position);
                _playerWayPointControl.BuildWayPoint();
            }
        }
    }
}
