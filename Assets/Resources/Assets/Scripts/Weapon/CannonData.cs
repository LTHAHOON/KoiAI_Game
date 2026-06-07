using UnityEngine;

[CreateAssetMenu(fileName = "new CannonData", menuName = "WeaponData/CannonData")]
public class CannonData : WeaponData
{
    [SerializeField]
    private CannonBallData _cannonBallData;
    [SerializeField]
    private float _aimSensitity = 10f;
    [SerializeField]
    private float _launchSpeed = 12f;
    [SerializeField] 
    private float _loadTime = 1f;
    [SerializeField] 
    private float _linearDamping = 0f;
        
    public float AimSensitity => _aimSensitity;
    public float LaunchSpeed => _launchSpeed;
    public float LinearDamping => _linearDamping;
    public CannonBallData CannonBallData => _cannonBallData;
    public float LoadTime => _loadTime;
}
