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
    private bool _hasHit;
    private Vector3 _lastPredictedPoint;
    private Vector2 _aim;
    private Pool<CannonBall> _pool;
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
            _remainingLoadCount = Mathf.Clamp(_remainingCount - _curLoadCount, 0, _remainingCount);
            _equipmentFeature.SetWeaponInfo(_curLoadCount, _remainingLoadCount);
            if (_curLoadTime >= _cannonData.LoadTime)
            {
                _equipmentFeature.SetWeaponInfo(_curLoadCount, _remainingLoadCount);

                _remainingLoadCount = 0;
                _curLoadCount = 0;
                _curCount = _remainingCount;
                _remainingCount = 0;
                _curLoadTime = 0f;
                _isFireLoading = false;
            }
        }
    }
    
    public override void Init(PlayerController itemOwner, Renderer itemUI ,ItemSlotType curSlotType)
    {
        base.Init(itemOwner, itemUI, curSlotType);
        #region PlayerEquipment 참조
        _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
        #endregion

        #region playerIA Setting
        PlayerInputAction playerIA = itemOwner.PlayerIA;
        playerIA.Player.Fire.performed += OnFire;
        playerIA.Player.FireLoad.performed += OnReLoadCannonBall;
        playerIA.Player.ProjectileAiming.performed += ProjectileAiming;
        playerIA.Player.ProjectileAiming.canceled += ProjectileAiming;
        #endregion
    }
    public override ItemData GetItemData()
    {
        return _cannonData;
    }

    public CannonData GetCannonData()
    {
        return _cannonData;
    }


    public override void UseItem()
    {
        if (!_equipmentFeature)
        {
            return;
        }

        #region 해당 아이템 장착
        _equipmentFeature.PushItemInSlot(this, ItemSlotType.Equipped);
        _equipmentFeature.EquipItem(this);
        #endregion

        #region 발사체 Item 하나 생성
        if (_equipmentFeature)
        {
            _equipmentFeature.CreateAndPushItemInSlot(ItemSlotType.NotEquipped, _cannonData.CannonBallData);
        }
        #endregion

        #region Projectile Pooling
        CannonBallData cannonBallData = _cannonData.CannonBallData;
        EntityId entityID = GetEntityId();
        CannonBall projectilePrefab = (CannonBall)cannonBallData.ItemPrefab;
        ulong id = EntityId.ToULong(entityID);
        PoolManager.Instance.AddPool<CannonBall>(id, projectilePrefab, cannonBallData.BulletPoolSize, PoolName.Bullet);
        PoolManager.Instance.TryGetPool<CannonBall>(id, out _pool);
        #endregion


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

    /// <summary>
    /// 발사체 장전 (CannonBall 아이템 사용)
    /// </summary>
    public void OnLoadCannonBall(CannonBallData cannonBallData)
    {
        //장전 중일때 차단
        if (_isFireLoading)
            return;
        _remainingCount += cannonBallData.ProjectileCount;
        _isFireLoading = true;
    }

    /// <summary>
    /// 발사체 재장전 (PlayerIA 연결)
    /// </summary>
    private void OnReLoadCannonBall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            /*
            Slot weaponSlot = _equipmentFeature.GetSelectedSlot(ItemSlotType.Equipped);
            Slot resourceSlot = _equipmentFeature.GetSelectedSlot(ItemSlotType.NotEquipped);

            ItemBase weapon = weaponSlot.GetItem();
            ItemBase resource = resourceSlot.GetItem();
            if (weapon == this && resource.GetItemData().ItemId == _cannonData.CannonBallData.ItemId)
            {
                _equipmentFeature.RemoveItemInSlot(resourceSlot, ItemSlotType.NotEquipped ,_cannonData.CannonBallData);

                _isFireLoading = true;
            }
            */
        }
    }
    
    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (_isFireLoading)
            {
                _curLoadTime = 0f;
                _equipmentFeature.SetWeaponInfo(_curLoadCount, _remainingLoadCount);
                _curCount = _curLoadCount;
                _remainingCount = _remainingLoadCount;
                _curLoadCount = 0;
                _remainingLoadCount = 0;
                _isFireLoading = false;
                return;
            }
            if (HasNotCannonBall())
                return;
            --_curCount;
            CannonBall bulletData = _pool.Pop();
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
            PoolManager.Instance.ReturnDelay(_pool, bulletData, _cannonData.CannonBallData.BulletLifeTime);
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
    
    private bool HasNotCannonBall() => _pool == null || _curCount <= 0;
}
