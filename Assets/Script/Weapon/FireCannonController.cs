using UnityEngine;
using UnityEngine.InputSystem;

public class FireCannonController : WeaponItemBase
{
    [SerializeField]
    private FireCannonData _fireCannonData;
    [SerializeField]
    private Transform _fireCannonPoint;
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    private float _aimSensitity = 10f;
    [SerializeField]
    private float _launchSpeed = 12f;
    [SerializeField]
    private float _pitchAngle = 35f;
    [SerializeField]
    private float _yawAngle = 0f;
    [SerializeField]
    private int _maxSteps = 40;
    [SerializeField]
    private float _timeStep = 0.08f;
    [SerializeField]
    private float _linearDamping = 0f;

    private Vector3 _hitPoint;
    private Vector3 _lastPredictedPoint;
    private bool _hasHit;
    private Vector2 _aim;
    private Pool<BulletData> _pool;
    
    public override ItemData GetItemData()
    {
        return _fireCannonData;
    }




    private void Start()
    {
        EntityId entityID = GetEntityId();
        BulletData bulletData = _projectilePrefab.GetComponent<BulletData>();
        ulong id = EntityId.ToULong(entityID);
        PoolManager.Instance.AddPool<BulletData>(id, bulletData, _fireCannonData.BulletPoolSize, PoolName.Bullet);
        PoolManager.Instance.TryGetPool<BulletData>(id, out _pool);
    }

    private void Update()
    {
        _pitchAngle += _aim.x * Time.deltaTime;
        _pitchAngle -= _aim.y * Time.deltaTime;
    }

    public void ProjectileAiming(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _aim = context.ReadValue<Vector2>() * _aimSensitity;
        }
        if(context.canceled)
        {
            _aim = Vector3.zero;
        }
    }

    private Vector3 GetLaunchVelocity()
    {
        Quaternion rotation = Quaternion.Euler(-_pitchAngle, _yawAngle, 0f);
        Vector3 localDir = rotation * Vector3.forward;
        Vector3 worldDirection = _fireCannonPoint.TransformDirection(localDir);
        return worldDirection * _launchSpeed;
    }

    public void FireProjectile(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (!_projectilePrefab)
                return;
            if (_pool == null)
                return;
            BulletData bulletData = _pool.Pop();
            bulletData.TrailRenderer.Clear();
            bulletData.TrailRenderer.enabled = false;
            Rigidbody bulletRigid = bulletData.Rigidbody;
            bulletRigid.transform.position = _fireCannonPoint.position;
            bulletRigid.transform.rotation = Quaternion.identity;
            bulletData.TrailRenderer.enabled = true;

            if (bulletRigid != null)
            {
                bulletRigid.linearDamping = _linearDamping;
                bulletRigid.useGravity = true;
                bulletRigid.linearVelocity = GetLaunchVelocity();
            }
            PoolManager.Instance.ReturnDelay(_pool, bulletData, _fireCannonData.BulletLifeTime);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        Vector3 velocity = GetLaunchVelocity();

        _hasHit = false;
        _lastPredictedPoint = position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 0.15f);
        for (int i = 0; i < _maxSteps; i++)
        {
            Vector3 previousPosition = position;
            velocity *= 1f - _linearDamping * _timeStep;
            velocity += Physics.gravity * _timeStep;
            position += velocity * _timeStep;

            Vector3 move = position - previousPosition;
            float distance = move.magnitude;

            if(Physics.Raycast(previousPosition, move.normalized, out RaycastHit hit, distance))
            {
                _hasHit = true;
                _hitPoint = hit.point;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(previousPosition, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.25f);
                break;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(previousPosition, hit.point);
            Gizmos.DrawWireSphere(position, 0.05f);
            _lastPredictedPoint = position;
        }

        if(!_hasHit)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_lastPredictedPoint, 0.25f);
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_hitPoint, 0.08f);
    }
}
