using Unity.Cinemachine;
using UnityEngine;

//무조건 UIFollowToObject보다 먼저 실행되게 설정
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