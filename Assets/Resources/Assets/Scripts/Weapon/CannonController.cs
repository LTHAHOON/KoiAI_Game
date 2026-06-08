using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : WeaponBase
{
    [SerializeField]
    private CannonData _cannonData;
    [SerializeField]
    private Transform _fireCannonPoint;
    [SerializeField]
    private float _pitchAngle = 35f;
    [SerializeField]
    private float _yawAngle = 0f;
    [SerializeField]
    private int _maxSteps = 40;
    [SerializeField]
    private float _timeStep = 0.08f;
    
    private Vector3 _hitPoint;
    private Vector3 _lastPredictedPoint;
    private bool _hasHit;
    private Vector2 _aim;
    private Pool<Projectile> _pool;
    private CannonBallData _cannonBallData;
    private Slot _cannonSlot;
    private int _curCount = 0;
    private int _curLoadCount;
    private int _remainingCount = 0;
    private int _remainingLoadCount = 0;

    private float _curLoadTime = 0f;
    private bool _isFireLoading = false;
    private readonly StringBuilder _sb = new();
    private PlayerEquipment _equipmentFeature;
    private void Update()
    {
        _pitchAngle += _aim.y * Time.deltaTime * 10f;
        _pitchAngle = Mathf.Clamp(_pitchAngle, _cannonData.MinPitchAngle, _cannonData.MaxPitchAngle);
        if (_isFireLoading)
        {
            _curLoadTime += Time.deltaTime;
            _curLoadCount = _curCount + Mathf.RoundToInt(_curLoadTime / _cannonData.LoadTime * _remainingCount);
            _remainingLoadCount = _remainingCount + Mathf.Clamp(_remainingCount - _curCount, 0, _remainingCount);
            _equipmentFeature.SetWeaponInfo(_curLoadCount, _remainingLoadCount);
            RefreshProjectile(_curLoadCount);
            if (_curLoadTime >= _cannonData.LoadTime)
            {
                _equipmentFeature.SetWeaponInfo(_curLoadCount, _remainingLoadCount);
                RefreshProjectile(_curCount);
                RemoveProjectileData();
                _curLoadCount = 0;
                _remainingLoadCount = 0;
                _curCount = _remainingCount;
                _remainingCount = 0;
                _curLoadTime = 0f;
                _isFireLoading = false;
            }
        }
    }
    
    public override void Init(PlayerController itemOwner)
    {
        SetItemOwner(itemOwner);

        #region playerIA Setting
        PlayerInputAction playerIA = itemOwner.PlayerIA;
        playerIA.Player.Fire.performed += OnFire;
        playerIA.Player.FireLoad.performed += OnFireLoad;
        playerIA.Player.ProjectileAiming.performed += ProjectileAiming;
        playerIA.Player.ProjectileAiming.canceled += ProjectileAiming;
        #endregion
        
        #region Projectile Connect
        _cannonBallData = _cannonData.CannonBallData;
        _curCount = 0;
        _remainingCount = _cannonBallData.Count;
        _equipmentFeature = (PlayerEquipment)itemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
        if(_equipmentFeature)
        {
            _cannonSlot = _equipmentFeature.AddItemInSlot(_cannonBallData);
            RefreshProjectile(_cannonBallData.Count);
        }
        #endregion
        
        #region Projectile Pooling
        EntityId entityID = GetEntityId();
        Projectile projectilePrefab = (Projectile)_cannonBallData.ItemPrefab;
        ulong id = EntityId.ToULong(entityID);
        PoolManager.Instance.AddPool<Projectile>(id, projectilePrefab, _cannonBallData.BulletPoolSize, PoolName.Bullet);
        PoolManager.Instance.TryGetPool<Projectile>(id, out _pool);
        #endregion
        
    }

    private void RefreshProjectile(int curProjectileCount)
    {
        if (!_cannonSlot)
            return;
        //탄이 없을 경우 숫자 X
        if (curProjectileCount <= 0)
        {
            _sb.Append("");
            _cannonSlot.SetItemCountText(_sb);
        }
        else
        {
            _sb.Append(curProjectileCount);
            _cannonSlot.SetItemCountText(_sb);    
        }                
        _sb.Clear();    
    }


    
    private void RemoveProjectileData()
    {
        if (!_equipmentFeature)
            return;
        _equipmentFeature.RemoveItemInSlot(_cannonSlot, _cannonBallData);
        _cannonSlot = null;
        _curCount = 0;
        _remainingCount = 0;
    }
        


    public void ProjectileAiming(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _aim = context.ReadValue<Vector2>() * _cannonData.AimSensitity;
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
        return worldDirection * _cannonData.LaunchSpeed;
    }


    private void OnFireLoad(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isFireLoading = true;
        }
    }
    
    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (HasNotCannonBall())
                return;
            if (_isFireLoading)
            {
                _curCount = _curLoadCount;
                _remainingCount = _remainingLoadCount;
                _curLoadTime = 0f;
                _curLoadCount = 0;
                _isFireLoading = false;
                return;
            }
            --_curCount;
            RefreshProjectile(_curCount);
            Projectile bulletData = _pool.Pop();
            bulletData.TrailRenderer.Clear();
            bulletData.TrailRenderer.enabled = false;
            Rigidbody bulletRigid = bulletData.Rigidbody;
            bulletRigid.transform.position = _fireCannonPoint.position;
            bulletRigid.transform.rotation = Quaternion.identity;
            bulletData.TrailRenderer.enabled = true;

            if (bulletRigid != null)
            {
                bulletRigid.linearDamping = _cannonData.LinearDamping;
                bulletRigid.useGravity = true;
                bulletRigid.linearVelocity = GetLaunchVelocity();
            }
            PoolManager.Instance.ReturnDelay(_pool, bulletData, _cannonBallData.BulletLifeTime);
            if (_curCount <= 0)
            {
                RefreshProjectile(_curCount);
            }
        }
    }
    
    
    private void OnDrawGizmos()
    {
        #region Gizmos
        Vector3 position = _fireCannonPoint.position;
        Vector3 velocity = GetLaunchVelocity();

        _hasHit = false;
        _lastPredictedPoint = position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 0.15f);

        
        for (int i = 0; i < _maxSteps; i++)
        {
            Vector3 previousPosition = position;
            velocity *= 1f - _cannonData.LinearDamping * _timeStep;
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
        #endregion
    }
    
    private bool HasNotCannonBall() => _pool == null || _cannonBallData == null || _curCount <= 0;

}
