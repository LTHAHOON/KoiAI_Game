using Unity.Cinemachine;
using UnityEngine;

public class SplineMover : MonoBehaviour
{
    [SerializeField]
    private CinemachineSplineDolly _splineDolly;
    [SerializeField]
    private float _speed;

    void Update()
    {
        _splineDolly.CameraPosition += Time.deltaTime;
        
    }

    
}
