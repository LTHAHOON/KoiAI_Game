using System;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.KoiCamera
{
    [CreateAssetMenu(fileName = "new CinemahcineOrbitalFollowData", menuName = "KoiAI/Camera/CinemahcineOrbitalFollowData")]
    public class CinemachineOrbitalFollowData : CinemachineData
    {
        [SerializeField]
        private float _radius;
        [SerializeField]
        private Vector3 _targetOffset;
        [SerializeField]
        private Vector2 _minMaxVerticalAxisRange;

        public override CinemachineCore.Stage GetCinemachineStage()
        {
            return CinemachineCore.Stage.Body;
        }

        public override Type GetCinemachineType()
        {
            return typeof(CinemachineOrbitalFollow);
        }

        public float Radius => _radius;
        public Vector3 TargetOffset => _targetOffset;
        public Vector2 MinMaxVerticalAxisRange => _minMaxVerticalAxisRange;
    }
}
