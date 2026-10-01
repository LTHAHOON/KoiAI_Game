using Unity.Cinemachine;

namespace KoiAI.KoiCamera
{
    public class CinemachineRotationComposerDataHandle : CinemachineDataHandle<CinemachineRotationComposer, CinemachineRotationComposerData>
    {
        public override void SetDataInComponent(CinemachineRotationComposer cmComponent, CinemachineRotationComposerData cmData)
        {
            cmComponent.Composition.ScreenPosition = cmData.ScreenPosition;
            cmComponent.TargetOffset = cmData.TargetOffset;
            cmComponent.Damping = cmData.Damping;
        }
    }
}
