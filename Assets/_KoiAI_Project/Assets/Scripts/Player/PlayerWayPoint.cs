using KoiAI.A_Star;
using KoiAI.Monster;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    [Serializable]
    public class PlayerWayPointValueData : PlayerFeatureValueData
    {
        [SerializeField]
        private WayPointData _wayPointData;
        [SerializeField]
        private float _buildDelayTime;

        public WayPointData WayPointData => _wayPointData;
        public float BuildDelayTime => _buildDelayTime;
    }

    [RequireComponent(typeof(EntitySight))]
    public class PlayerWayPoint : PlayerFeature
    {
        [SerializeField]
        private Transform _target;
        [ReadOnly]
        [SerializeField]
        private PlayerWayPointValueData _valueData;

        private EntitySight _entitySight;
        private readonly WayPointHandler _wayPointHandle = new();
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
            _entitySight = GetComponent<EntitySight>(); 
            _valueData = valueData;
            playerIA.Player.SetVisibleWayPoint.performed += OnSetVisibleWayPoint;
            playerIA.Player.Move.performed += OnRebuildWayPoint;
            playerIA.Player.Move.canceled += OnRebuildWayPoint;
        }

        public bool DetectTargetAroundPlayer()
        {
            _entitySight.Detect();
            if(_entitySight.IsFindTarget())
            {
                _target = _entitySight.GetTargetToFind().transform;
                return true;
            }
            return false;
        }

        public void OnSetVisibleWayPoint(InputAction.CallbackContext context)
        {
            bool isFindTarget = DetectTargetAroundPlayer();
            if(!isFindTarget)
            {
                return;
            }
            if (context.performed)
            {
                _isShowedWayPoint = !_isShowedWayPoint;
                if (_wayPointHandle.IsBuilding)
                {
                    return;
                }
                _wayPointHandle.InitStartAndGoal(this, _valueData.WayPointData, transform.position, _target.position);
                _wayPointHandle.BuildOrClearWayPoint(_isShowedWayPoint);
            }
        }

        public void OnRebuildWayPoint(InputAction.CallbackContext context)
        {
            if (_isShowedWayPoint == false || _wayPointHandle.IsBuilding)
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
                if(!_target)
                {
                    return;
                }
                _wayPointHandle.InitStartAndGoal(this, _valueData.WayPointData, transform.position, _target.position);
                _wayPointHandle.BuildWayPoint();
            }
        }
    
        public override void UpdateFeature()
        {
            if (!_target || _isShowedWayPoint == false || _wayPointHandle.IsBuilding)
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
                _wayPointHandle.InitStartAndGoal(this, _valueData.WayPointData, transform.position, _target.position);
                _wayPointHandle.BuildWayPoint();
            }
        }
    }
}
