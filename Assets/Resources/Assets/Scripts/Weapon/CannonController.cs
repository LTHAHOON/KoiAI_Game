using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : WeaponControllerBase
{
    [SerializeField]
    private float _pitchAngle = 35f;
    [SerializeField]
    private float _yawAngle = 0f;
    [SerializeField]
    private int _maxSteps = 40;
    [SerializeField]
    private float _timeStep = 0.08f;
    [SerializeField]
    private CannonData _cannonData;

    private SurfaceAngleFinder _surfaceAngleFinder;
    private GameObject _aimObj;
    private CannonSkin _cannonSkin;
    private Pool<CannonBallItem> _pool;
    private Vector3 _hitPoint;
    private bool _hasHit;
    private Vector3 _lastPredictedPoint;

    private int _curBallCount = 0;
    private int _curBallLoadCount;
    private int _remainingBallCount = 0;
    private int _remainingBallLoadCount = 0;
    private float _curLoadTime = 0f;
    private float _targetLoadTime = 0f;
    private bool _isFireLoading = false;

    private void Update()
    {
        if (!_cannonSkin)
        {
            return;
        }
        if (_isFireLoading)
        {
            #region 발사체 장전 로직
            if(_targetLoadTime <= 0f)
            {
                _curBallLoadCount = 0;
                _remainingBallLoadCount = 0;
                _isFireLoading = false;
                return;
            }
            _curLoadTime += Time.deltaTime;
            int count = Mathf.RoundToInt(_curLoadTime / _targetLoadTime * _remainingBallCount);
            _curBallLoadCount = _curBallCount + count;
            _remainingBallLoadCount = Mathf.Clamp(_remainingBallCount - count, 0, _remainingBallCount);

            if (_curLoadTime >= _targetLoadTime || _curBallLoadCount >= _cannonData.LoadMaxCount)
            {
                _curBallCount = _curBallLoadCount;
                _remainingBallCount = _remainingBallLoadCount;
                _curLoadTime = 0f;
                _targetLoadTime = 0f;
            }
            #endregion
        }
        ShowAiming();
    }

    public override void Init()
    {
        CannonBallData cannonBallData = _cannonData.CannonBallData;
        EntityId entityID = GetEntityId();
        CannonBallItem projectilePrefab = (CannonBallItem)cannonBallData.ItemPrefab;
        ulong id = EntityId.ToULong(entityID);
        PoolManager.Instance.AddPool<CannonBallItem>(id, projectilePrefab, cannonBallData.CannonBallPoolSize, PoolName.Bullet);
        PoolManager.Instance.TryGetPool<CannonBallItem>(id, out _pool);
        CannonBallItem[] cannonBalls = _pool.GetAllInstanceWithoutPop();
        for (int i = 0; i < cannonBalls.Length; i++)
        {
            //발사체 스킨 생성
            cannonBalls[i].ChangeSkin();
        }
        _aimObj = Instantiate(_cannonData.AimPrefab);
        _surfaceAngleFinder = new(5);
    }

    public override bool Activate()
    {
        if (_isFireLoading)
        {
            #region 장전 중일 경우 장전된 만큼 설정하고 초기화하고 True 리턴하기
            _curLoadTime = 0f;
            _targetLoadTime = 0;
            _curBallCount = _curBallLoadCount;
            _remainingBallCount = _remainingBallLoadCount;
            _curBallLoadCount = 0;
            _remainingBallLoadCount = 0;
            _isFireLoading = false;
            return true;
            #endregion
        }
        if (HasNotCannonBall())
        {
            return false;
        }

        --_curBallCount;
        CannonBallItem bulletData = _pool.Pop();
        if (bulletData.IsEmptySkin())
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
        return true;
    }
    public override void SetAim(Vector3 aim)
    {
        _pitchAngle += -aim.y * Time.deltaTime * 10f;
        _pitchAngle = Mathf.Clamp(_pitchAngle, _cannonData.MinPitchAngle, _cannonData.MaxPitchAngle);
    }

    public void OnLoadCannonBall(CannonBallData cannonBallData)
    {
        if (_curBallCount >= _cannonData.LoadMaxCount)
        {
            _remainingBallCount += cannonBallData.ProjectileCount;
            return;
        }
        _targetLoadTime += _cannonData.LoadTime;
        _remainingBallCount += cannonBallData.ProjectileCount;
        _isFireLoading = true;
    }

    public void OnReLoadCannonBall()
    {
        //중복 장전 차단
        if (_isFireLoading)
        {
            return;
        }
        //탄이 꽉 차있거나 남은 탄이 없을 경우 리턴
        if (_curBallCount >= _cannonData.LoadMaxCount || _remainingBallCount <= 0)
        {
            return;
        }
        _targetLoadTime = _cannonData.LoadTime * _remainingBallCount / _cannonData.LoadMaxCount;
        _isFireLoading = true;
    }

    public Vector3 GetLaunchVelocity()
    {
        if (!_cannonSkin)
            return Vector3.zero;
        Quaternion rotation = Quaternion.Euler(-_pitchAngle, _yawAngle, 0f);
        Vector3 localDir = rotation * Vector3.forward;
        Vector3 worldDirection = _cannonSkin.FirePoint.TransformDirection(localDir);
        return worldDirection * _cannonData.LaunchSpeed;
    }

    public override void ChangeSkin()
    {
        if (_cannonSkin)
        {
            Destroy(_cannonSkin);
        }
        _cannonSkin = Instantiate(_cannonData.SkinData, transform);
    }

    public void ShowAiming()
    {
        if(!_aimObj)
        {
            return;
        }
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

            if (Physics.Raycast(previousPosition, move.normalized, out RaycastHit hit, distance, _cannonData.LayerMaskForAim))
            {
                _hasHit = true;
                _hitPoint = hit.point;
                _hitPoint.y += 0.3f;
                break;
            }

            _lastPredictedPoint = position;
        }
        //해당 Aim오브젝트는 Local, World 축 차이가 없기 때문에 world기준으로 구하기
        _surfaceAngleFinder.TryGetWorldSurfaceAngle(out Vector3 angleVec, _aimObj.transform);
        Quaternion aimRotation = Quaternion.Euler(angleVec);
        _aimObj.transform.eulerAngles =angleVec;
        _aimObj.transform.position = _hitPoint;
    }

    public bool IsFireLoading() => _isFireLoading;

    private bool HasNotCannonBall() => _pool == null || _curBallCount <= 0;
    public CannonData CannonData => _cannonData;
    public CannonSkin CannonSkin => _cannonSkin;
    public int CurBallCount => _curBallCount;
    public int RemainingBallCount => _remainingBallCount;
    public int CurBallLoadCount => _curBallLoadCount;
    public int RemainingBallLoadCount => _remainingBallLoadCount;
}
