using Unity.Cinemachine;
using UnityEngine;

public class CMCameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera _cmDoolySpline;
    private void Start()
    {
        _cmDoolySpline.Priority = 9;
    }

}
