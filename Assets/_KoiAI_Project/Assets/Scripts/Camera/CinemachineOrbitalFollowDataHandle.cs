using KoiAI.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace KoiAI.Camera
{
    public class CinemachineOrbitalFollowDataHandle : CinemachineDataHandle<CinemachineOrbitalFollow, CinemachineOrbitalFollowData>
    {
        public override void SetDataInComponent(CinemachineOrbitalFollow cmComponent, CinemachineOrbitalFollowData cmData)
        {
            cmComponent.Radius = cmData.Radius;
        }
    }
}
