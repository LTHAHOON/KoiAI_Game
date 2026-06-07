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
    private int _curProjectileCount = 0;
    private int _maxProjectileCount = 0;
    private float _curLoadTime = 0f;
    private bool _isFireLoading = false;
    private readonly StringBuilder _sb = new();
    
    private void Update()
    {
        _pitchAngle += _aim.x * Time.deltaTime;
        _pitchAngle -= _aim.y * Time.deltaTime;

        if (_isFireLoading)
        {
            _curLoadTime += Time.deltaTime;
            _curProjectileCount = Mathf.RoundToInt(_curLoadTime / _cannonData.LoadTime * _maxProjectileCount);
            if (_curLoadTime >= _cannonData.LoadTime)
            {
                _curProjectileCount = _maxProjectileCount;
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
        _curProjectileCount = 0;
        _maxProjectileCount = _cannonBallData.Count;
        var playerFeature = (PlayerEquipment)itemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
        if(playerFeature)
        {
            _cannonSlot = playerFeature.AddItemInSlot(_cannonBallData);
            RefreshProjectileCount(_cannonBallData.Count);
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

    private void RefreshProjectileCount(int curProjectileCount)
    {
        if (!_cannonSlot)
            return;
        //탄이 없을 경우 탄 연결 해제(탄 삭제)
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
        var playerFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
        playerFeature?.RemoveItemInSlot(_cannonSlot, _cannonBallData);
        _cannonSlot = null;
        _curProjectileCount = 0;
        _maxProjectileCount = 0;
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
            if (_cannonSlot == null)
            {
                //CannonBall 찾기
                return;
            }
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
                _curLoadTime = 0f;
                _isFireLoading = false;
            }
            --_curProjectileCount;
            RefreshProjectileCount(_curProjectileCount);
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
            if (_curProjectileCount <= 0)
            {
                RefreshProjectileCount(_curProjectileCount);
                RemoveProjectileData();
            }
        }
    }
    
    
    private void OnDrawGizmos()
    {
        #region Gizmos
        Vector3 position = transform.position;
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
    
    private bool HasNotCannonBall() => _pool == null || _cannonBallData == null || _cannonSlot == null 
                                       || _cannonData == null || _curProjectileCount <= 0;

}
