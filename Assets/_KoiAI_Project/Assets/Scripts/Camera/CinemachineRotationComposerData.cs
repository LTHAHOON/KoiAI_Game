using System;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.Camera
{
    [CreateAssetMenu(fileName = "new CinemachineRotationComposerData", menuName = "KoiAI/Camera/CinemachineRotationComposerData")]
    public class CinemachineRotationComposerData : CinemachineData
    {
        [SerializeField]
        private Vector3 _screenPosition = Vector3.zero;
        [SerializeField]
        private Vector3 _targetOffset = new(0, 1, 0);
        [SerializeField]
        private Vector2 _damping = new(0.5f, 0.5f);

        public override CinemachineCore.Stage GetCinemachineStage()
        {
            return CinemachineCore.Stage.Aim;
        }

        public override Type GetCinemachineType()
        {
            return typeof(CinemachineRotationComposer);
        }

        public Vector3 ScreenPosition => _screenPosition;
        public Vector3 TargetOffset => _targetOffset;
        public Vector2 Damping => _damping;
    }
}
