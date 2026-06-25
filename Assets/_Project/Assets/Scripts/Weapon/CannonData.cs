using UnityEngine;

[CreateAssetMenu(fileName = "new CannonData", menuName = "WeaponData/CannonData")]
public class CannonData : WeaponData
{
    [SerializeField]
    private LayerMask _layerMaskForAim;
    [SerializeField]
    private CannonAim _cannonAimPrefab;
    [SerializeField]
    private CannonBallData _cannonBallData;
    [SerializeField]
    private bool _isInfiniteLoad = false;
    [SerializeField]
    private int _loadMaxCount = 20;
    [SerializeField]
    private float _aimSensitity = 10f;
    [SerializeField]
    private float _launchSpeed = 12f;
    [SerializeField] 
    private float _loadTime = 1f;
    [SerializeField] 
    private float _linearDamping = 0f;
    [SerializeField]
    private float _maxPitchAngle = 90f;
    [SerializeField]
    private float _minPitchAngle = 0f;
    [SerializeField]
    private float _maxYawAngle = 90f;
    [SerializeField]
    private float _minYawAngle = -90f;

    public bool IsInfiniteLoad => _isInfiniteLoad;
    public float MaxYawAngle => _maxYawAngle;
    public float MinYawAngle => _minYawAngle;
    public LayerMask LayerMaskForAim => _layerMaskForAim;
    public CannonAim AimPrefab => _cannonAimPrefab;
    public int LoadMaxCount => _loadMaxCount;
    public float AimSensitity => _aimSensitity;
    public float LaunchSpeed => _launchSpeed;
    public float LinearDamping => _linearDamping;
    public CannonBallData CannonBallData => _cannonBallData;
    public float LoadTime => _loadTime;
    public float MaxPitchAngle => _maxPitchAngle;
    public float MinPitchAngle => _minPitchAngle;
}
