using Unity.Cinemachine;
using UnityEngine;

//무조건 UIFollowToObject보다 먼저 실행되게 설정(카메라 위치를 이용한 UI 떨림 현상 방지)
namespace KoiAI.Camera
{
    [DefaultExecutionOrder(100)]
    public class CameraManualUpdater : MonoBehaviour
    {
        [SerializeField]
        private CinemachineBrain cinemachineBrain;

        private void LateUpdate()
        {
            if (cinemachineBrain == null)
            {
                return;
            }
            CinemachineCore.UniformDeltaTimeOverride = Time.deltaTime;
            cinemachineBrain.ManualUpdate();
        }
    }
}