using System;
using KoiAI.A_Star;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    [Serializable]
    public class PlayerWayPointValueData : PlayerFeatureValueData
    {
        [SerializeField]
        private float _buildDelayTime;
        
        public float BuildDelayTime => _buildDelayTime;
    }
    
    public class PlayerWayPoint : PlayerFeature
    {
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private WayPointHandle _playerWayPointControl;
        [SerializeField]
        private PlayerWayPointValueData _valueData;

        private float _curTime = 0;
        private bool _isAutoBuild = false;
        private bool _isShowedWayPoint = false;
        public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.WayPoint;

        public override void Init(PlayerInputAction playerIA, PlayerFeatureValueData playerFeatureValueData = null, 
            PlayerFeatureExtensionData playerFeatureExtensionData = null)
        {
            if (playerIA == null)
            {
                return;
            }
            if (playerFeatureValueData is not PlayerWayPointValueData valueData)
            {
                return;
            }
            _valueData = valueData;
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
                if(_curTime < _valueData.BuildDelayTime)
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
