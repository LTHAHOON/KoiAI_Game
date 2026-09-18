using System;
using UnityEngine;

namespace KoiAI.AI
{
    using KoiAI.Utilities;
    using R3;

    [Serializable]
    public class AIRotationExtensionData : AIFeatureExtensionData
    {
        #region 보정값 및 추가 회전 데이터

        [SerializeField]
        private float _lookSpeedMod;
        [SerializeField]
        private float _surfaceCheckDistanceMod;
        [SerializeField]
        private LayerMask _surfaceLayerMask;

        #endregion
        public float LookSpeedMod => _lookSpeedMod;
        public float SurfaceCheckDistanceMod => _surfaceCheckDistanceMod;
        public LayerMask SurfaceLayerMask => _surfaceLayerMask;
    }

    [Serializable]
    public class AIRotationValueData : AIFeatureValueData
    {
        #region 회전 데이터

        [SerializeField]
        private float _lookSpeed = 10f;
        [SerializeField]
        private float _surfaceCheckDistance = 3f;

        #endregion
        public float LookSpeed => _lookSpeed;
        public float SurfaceCheckDistance => _surfaceCheckDistance;
    }

    public class AIRotation : AIFeature
    {
        private AIRotationValueData _valueData;
        private AIRotationExtensionData _extensionData;

        private SurfaceAngleFinder _surfaceAngleFinder;
        private Vector3 _targetAngle = Vector3.zero;
        private GameObject _target;
        private bool _bHasTarget = false;

        public override void InitFeature(AIFeatureValueData aiFeatureValueData = null,
            AIFeatureExtensionData aiFeatureExtensionData = null)
        {
            if (aiFeatureValueData is not AIRotationValueData valueData
             || aiFeatureExtensionData is not AIRotationExtensionData extensionData)
            {
                return;
            }
            _valueData = valueData;
            _extensionData = extensionData;
            _surfaceAngleFinder = new(_valueData.SurfaceCheckDistance + _extensionData.SurfaceCheckDistanceMod);

            Observable.EveryUpdate(UnityFrameProvider.FixedUpdate).Where(_ => _bHasTarget).Subscribe(_ =>
            {
                Quaternion quat = Quaternion.Euler(_targetAngle.x, _targetAngle.y, _targetAngle.z);
                float speed = _valueData.LookSpeed + _extensionData.LookSpeedMod;

                //RigidPhysicsUpdate타입일 경우 Rigid회전 적용
                if(Brain.AgentController.Rigidbody)
                {
                    Brain.AgentController.Rigidbody.MoveRotation(Quaternion.Slerp(Brain.transform.rotation, quat, Time.fixedDeltaTime * speed));
                }
                else
                {
                    Brain.transform.rotation = Quaternion.Slerp(Brain.transform.rotation, quat, Time.fixedDeltaTime * speed);   
                }
            }).AddTo(Brain);
        }

        public override void EnterFeature()
        {
            _bHasTarget = TryGetTarget(out _target);
        }

        public override void UpdateFeature()
        {
            _surfaceAngleFinder.TryGetWorldSurfaceAngle(out _targetAngle, Brain.transform);

            Vector3 dir;
            if (_bHasTarget)
            {
                if (Brain.IsFeatureActive(AIFeatureProperty.Return))
                {
                    dir = Brain.OriginPosition - Brain.transform.position;
                }
                else
                {
                    dir = _target.transform.position - Brain.transform.position;
                }
            }
            else
            {
                dir = Brain.OriginPosition - Brain.transform.position;
            }

            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            _targetAngle.y = angle;
        }

        public override void ExitFeature()
        {
            _bHasTarget = false;
        }
    }
}
