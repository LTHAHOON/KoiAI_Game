using System.Collections;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : WeaponBase
{
    [SerializeField]
    private CannonData _cannonData;
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
    private int _curBallCount = 0;
    private int _curBallLoadCount;
    private int _remainingBallCount = 0;
    private int _remainingBallLoadCount = 0;

    private float _curLoadTime = 0f;
    private float _targetLoadTime = 0f;
    private bool _isFireLoading = false;
    private PlayerEquipment _equipmentFeature;
    private CannonSkin _cannonSkin;
    private void Update()
    {
        if(!_cannonSkin)
        {
            return;
        }

        _pitchAngle += _aim.y * Time.deltaTime * 10f;
        _pitchAngle = Mathf.Clamp(_pitchAngle, _cannonData.MinPitchAngle, _cannonData.MaxPitchAngle);
        
        if (_isFireLoading)
        {
            #region 발사체 장전 로직
            _curLoadTime += Time.deltaTime;
            int count = Mathf.RoundToInt(_curLoadTime / _targetLoadTime * _remainingBallCount);
            _curBallLoadCount = _curBallCount + count;
            _remainingBallLoadCount = Mathf.Clamp(_remainingBallCount - count, 0, _remainingBallCount);
            _equipmentFeature.SetWeaponInfo(_curBallLoadCount, _remainingBallLoadCount);
            if (_curLoadTime >= _cannonData.LoadTime || _curBallLoadCount >= _cannonData.LoadMaxCount)
            {
                _equipmentFeature.SetWeaponInfo(_curBallLoadCount, _remainingBallLoadCount);
                _curBallCount = _curBallLoadCount;
                _remainingBallCount = _remainingBallLoadCount;
                _curBallLoadCount = 0;
                _remainingBallLoadCount = 0;
                _targetLoadTime = 0f;
                _curLoadTime = 0f;
                _isFireLoading = false;
            }
            #endregion
        }
    }

    private void OnEnable()
    {
        var curSlotType = GetCurrentSlotType();
        if (curSlotType == ItemSlotType.Equipped)
        {
            _equipmentFeature.SetWeaponInfo(_curBallCount, _remainingBallCount);
        }
    }

    private void OnDestroy()
    {
        var curSlotType = GetCurrentSlotType();
        if (curSlotType == ItemSlotType.Equipped)
        {
            PlayerInputAction playerIA = ItemOwner.PlayerIA;
            DisConnectPlayerIA(ItemOwner.PlayerIA);
        }
    }

    /// <summary>
    /// 아이템 초기화(본체를 생성하기 전 세팅)
    /// </summary>
    public override void Init(PlayerController itemOwner, Renderer itemUI ,ItemSlotType curSlotType)
    {
        base.Init(itemOwner, itemUI, curSlotType);
        #region PlayerEquipment 참조
        _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
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
        //장착된 무기들중 같은 무기가 있는지 한번더 체크(있으면 장착하지 않고 파기)
        bool bExistSameItem = _equipmentFeature.IsExistSameID(this, ItemSlotType.Equipped);
        #region playerIA Setting
        if(!bExistSameItem)
        {
            ConnectPlayerIA(ItemOwner.PlayerIA);
        }
        #endregion

        #region 해당 아이템 장착
        if (!bExistSameItem)
        {
            ChangeSkin();
            _equipmentFeature.PushItemInSlot(this, ItemSlotType.Equipped);
            _equipmentFeature.EquipItem(this);
        }
        #endregion

        #region 발사체 Item 하나 생성
        if (_equipmentFeature)
        {
            _equipmentFeature.CreateAndPushItemInSlot(ItemSlotType.NotEquipped, _cannonData.CannonBallData);
        }
        #endregion

        #region Projectile Pooling
        if (!bExistSameItem)
        {
            CannonBallData cannonBallData = _cannonData.CannonBallData;
            EntityId entityID = GetEntityId();
            CannonBall projectilePrefab = (CannonBall)cannonBallData.ItemPrefab;
            ulong id = EntityId.ToULong(entityID);
            PoolManager.Instance.AddPool<CannonBall>(id, projectilePrefab, cannonBallData.CannonBallPoolSize, PoolName.Bullet);
            PoolManager.Instance.TryGetPool<CannonBall>(id, out _pool);
            var cannonBalls = _pool.GetAllInstanceWithoutPop();
            for (int i = 0; i < cannonBalls.Length; i++)
            {
                //발사체 스킨 생성
                cannonBalls[i].ChangeSkin();
            }
        }
        #endregion

        if(bExistSameItem)
        {
            _equipmentFeature.RemoveItemInSlot(this);
        }
    }

    public void ChangeSkin()
    {
        if (_cannonSkin)
        {
            Destroy(_cannonSkin);
        }
        _cannonSkin = Instantiate(_cannonData.SkinData, transform);
    }

    private Vector3 GetLaunchVelocity()
    {
        if (!_cannonSkin)
            return Vector3.zero;
        Quaternion rotation = Quaternion.Euler(-_pitchAngle, _yawAngle, 0f);
        Vector3 localDir = rotation * Vector3.forward;
        Vector3 worldDirection = _cannonSkin.FirePoint.TransformDirection(localDir);
        return worldDirection * _cannonData.LaunchSpeed;
    }

    /// <summary>
    /// 발사체 장전
    /// </summary>
    public void OnLoadCannonBall(CannonBallData cannonBallData)
    {
        if(_curBallCount >= _cannonData.LoadMaxCount)
        {
            _remainingBallCount += cannonBallData.ProjectileCount;
            _equipmentFeature.SetWeaponInfo(_curBallCount, _remainingBallCount);
            return;
        }
        _targetLoadTime += _cannonData.LoadTime;
        _remainingBallCount += cannonBallData.ProjectileCount;
        _isFireLoading = true;
    }

    /// <summary>
    /// 발사체 재장전
    /// </summary>
    private void OnReLoadCannonBall(InputAction.CallbackContext context)
    {
        //중복 장전 차단
        if(_isFireLoading)
        {
            return;
        }    
        if (context.performed)
        {
            if (_curBallCount >= _cannonData.LoadMaxCount)
            {
                return;
            }
            _targetLoadTime = _cannonData.LoadTime;
            _isFireLoading = true;
        }
    }

    public void OnProjectileAiming(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _aim = context.ReadValue<Vector2>() * _cannonData.AimSensitity;
        }
        if (context.canceled)
        {
            _aim = Vector3.zero;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if(gameObject.activeSelf == false || !_cannonSkin)
        {
            return;
        }
        if(context.performed)
        {
            if (_isFireLoading)
            {
                #region 장전 중일 경우 장전된 만큼 설정하고 초기화하고 리턴하기
                _equipmentFeature.SetWeaponInfo(_curBallLoadCount, _remainingBallLoadCount);
                _curLoadTime = 0f;
                _targetLoadTime = 0;
                _curBallCount = _curBallLoadCount;
                _remainingBallCount = _remainingBallLoadCount;
                _curBallLoadCount = 0;
                _remainingBallLoadCount = 0;
                _isFireLoading = false;
                return;
                #endregion
            }
            if (HasNotCannonBall())
                return;
            --_curBallCount;
            _equipmentFeature.SetWeaponInfo(_curBallCount, _remainingBallCount);
            CannonBall bulletData = _pool.Pop();
            if(bulletData.IsEmptySkin())
            {
                bulletData.ChangeSkin();
            }
            bulletData.TrailRenderer.Clear();
            bulletData.TrailRenderer.enabled = false;
            Rigidbody bulletRigid = bulletData.Rigidbody;
            bulletRigid.transform.position = _cannonSkin.FirePoint.position;
            bulletRigid.transform.rotation = Quaternion.identity;
            bulletData.TrailRenderer.enabled = true;

            if (bulletRigid != null)
            {
                bulletRigid.linearDamping = _cannonData.LinearDamping;
                bulletRigid.useGravity = true;
                bulletRigid.linearVelocity = GetLaunchVelocity();
            }
            PoolManager.Instance.ReturnDelay(_pool, bulletData, _cannonData.CannonBallData.CannonBallLifeTime);
        }
    }
    
    public void ShowAiming()
    {
        Vector3 position = _cannonSkin.FirePoint.position;
        Vector3 velocity = GetLaunchVelocity();

        _hasHit = false;
        _lastPredictedPoint = position;

        for (int i = 0; i < _maxSteps; i++)
        {
            Vector3 previousPosition = position;
            velocity *= 1f - _cannonData.LinearDamping * _timeStep;
            velocity += Physics.gravity * _timeStep;
            position += velocity * _timeStep;

            Vector3 move = position - previousPosition;
            float distance = move.magnitude;

            if (Physics.Raycast(previousPosition, move.normalized, out RaycastHit hit, distance))
            {
                _hasHit = true;
                _hitPoint = hit.point;
                break;
            }

            Gizmos.color = Color.cyan;
            _lastPredictedPoint = position;
        }

        if (!_hasHit)
        {
            return;
        }
    }

    private void OnDrawGizmos()
    {
        #region Gizmos
        if (!_cannonSkin)
            return;
        Vector3 position = _cannonSkin.FirePoint.position;
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

    protected override void ConnectPlayerIA(PlayerInputAction playerIA)
    {
        playerIA.Player.Fire.performed += OnFire;
        playerIA.Player.FireLoad.performed += OnReLoadCannonBall;
        playerIA.Player.ProjectileAiming.performed += OnProjectileAiming;
        playerIA.Player.ProjectileAiming.canceled += OnProjectileAiming;
    }

    protected override void DisConnectPlayerIA(PlayerInputAction playerIA)
    {
        playerIA.Player.Fire.performed -= OnFire;
        playerIA.Player.FireLoad.performed -= OnReLoadCannonBall;
        playerIA.Player.ProjectileAiming.performed -= OnProjectileAiming;
        playerIA.Player.ProjectileAiming.canceled -= OnProjectileAiming;
    }

    private bool HasNotCannonBall() => _pool == null || _curBallCount <= 0;

}
