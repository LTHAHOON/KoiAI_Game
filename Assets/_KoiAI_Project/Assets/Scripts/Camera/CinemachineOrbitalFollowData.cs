using System;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.Camera
{
    [CreateAssetMenu(fileName = "new CinemahcineOrbitalData", menuName = "KoiAI/Camera/CinemahcineOrbitalData")]
    public class CinemachineOrbitalFollowData : CinemachineData
    {
        [SerializeField]
        private float _radius;

        public override CinemachineCore.Stage GetCinemachineStage()
        {
            return CinemachineCore.Stage.Body;
        }

        public override Type GetCinemachineType()
        {
            return typeof(CinemachineOrbitalFollow);
        }

        public float Radius => _radius;
    }
}
