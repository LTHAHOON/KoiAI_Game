using R3;
using System.Buffers;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerSFXAudioFeature;

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
    [SerializeField] 
    private LayerMask _targetLayerMask;
    
    private SurfaceAngleFinder _surfaceAngleFinder;
    private CannonAim _cannonAim;
    private CannonSkin _cannonSkin;
    private Pool<CannonBallItem> _pool;
    private Vector3 _hitPoint;

    private readonly ReactiveProperty<int> _curBallCount = new(0);
    private AudioSFXTarget _attackAuidoTarget;
    private int _curBallLoadCount;
    private int _remainingBallCount = 0;
    private int _remainingBallLoadCount = 0;
    private float _curLoadTime = 0f;
    private float _targetLoadTime = 0f;
    private bool _isFireLoading = false;
    private bool _isAiming = false;

    private void Update()
    {
        if (!_cannonSkin)
        {
            return;
        }
        if (IsFireLoading())
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
            _curBallLoadCount = _curBallCount.CurrentValue + count;
            _remainingBallLoadCount = Mathf.Clamp(_remainingBallCount - count, 0, _remainingBallCount);

            if (_curLoadTime >= _targetLoadTime || _curBallLoadCount >= _cannonData.LoadMaxCount)
            {
                _curBallCount.Value = _curBallLoadCount;
                _remainingBallCount = _remainingBallLoadCount;
                _curLoadTime = 0f;
                _targetLoadTime = 0f;
            }
            #endregion
        }
        if(IsAiming())
        {
            ShowAiming();
        }
    }

    public override void Init(WeaponBase wepaonItem)
    {
        _curBallCount
            .Pairwise()
            .Where(pair => pair.Current < pair.Previous)
            .Subscribe(_ =>
            {
                if(_cannonSkin.FirePT)
                {
                    _cannonSkin.FirePT.Play();
                }
                if(_cannonSkin.FireAudioData)
                {
                    var owner = wepaonItem.ItemOwner;
                    if(_attackAuidoTarget == null)
                    {
                        _attackAuidoTarget = owner.GetAudioSFXTarget(PlayerSFXAuidoProperty.Attack);
                    }
                    AudioManager.Instance.PlaySFX(_attackAuidoTarget, _cannonSkin.FireAudioData, _cannonSkin.FirePoint.position);
                }
            }).AddTo(this);

        CannonBallData cannonBallData = _cannonData.CannonBallData;
        EntityId entityID = GetEntityId();
        CannonBallItem projectilePrefab = (CannonBallItem)cannonBallData.ItemPrefab;
        ulong id = EntityId.ToULong(entityID);
        PoolManager.Instance.AddPool<CannonBallItem>(id, projectilePrefab, cannonBallData.CannonBallPoolSize, PoolName.Projectile);
        PoolManager.Instance.TryGetPool<CannonBallItem>(id, out _pool);
        CannonBallItem[] cannonBalls = _pool.GetAllInstanceArray();
        for (int i = 0; i < cannonBalls.Length; i++)
        {
            //발사체 스킨 생성
            cannonBalls[i].SetupController(_targetLayerMask);
        }
        _cannonAim = Instantiate(_cannonData.AimPrefab, transform);
        _cannonAim.gameObject.SetActive(false);
        _surfaceAngleFinder = new(5);
        InitSkin();
    }

    public override bool Activate()
    {
        if (_isFireLoading)
        {
            #region 장전 중일 경우 장전된 만큼 설정하고 초기화하고 True 리턴하기
            _curLoadTime = 0f;
            _targetLoadTime = 0;
            _curBallCount.Value = _curBallLoadCount;
            _remainingBallCount = _remainingBallLoadCount;
            _curBallLoadCount = 0;
            _remainingBallLoadCount = 0;
            _isFireLoading = false;
            return true;
            #endregion
        }
        if (HasNotCannonBall() && !_cannonData.IsInfiniteLoad)
        {
            return false;
        }

        if (!_cannonData.IsInfiniteLoad)
        {
            --_curBallCount.Value;
        }

        CannonBallItem bulletData = _pool.Pop();
        if(bulletData == null)
        {
            return false;
        }
        if (bulletData.IsEmptyController())
        {
            bulletData.SetupController(_targetLayerMask);
        }
        bulletData.TrailRenderer.Clear();
        bulletData.TrailRenderer.enabled = false;
        bulletData.TrailRenderer.enabled = true;
        Rigidbody bulletRigid = bulletData.Rigidbody;
        if (bulletRigid == null)
        {
            return false;
        }
        bulletRigid.transform.position = _cannonSkin.FirePoint.position;
        bulletRigid.linearDamping = _cannonData.LinearDamping;
        bulletRigid.useGravity = true;
        bulletRigid.linearVelocity = GetLaunchVelocity();
        PoolManager.Instance.ReturnDelay(_pool, bulletData, _cannonData.CannonBallData.CannonBallLifeTime);
        return true;
    }
    public override void SetAim(Vector2 aim)
    {
        _yawAngle += aim.x * Time.deltaTime * 10f;
        _yawAngle = Mathf.Clamp(_yawAngle, _cannonData.MinYawAngle, _cannonData.MaxYawAngle);
        _pitchAngle += -aim.y * Time.deltaTime * 10f;
        _pitchAngle = Mathf.Clamp(_pitchAngle, _cannonData.MinPitchAngle, _cannonData.MaxPitchAngle);
    }

    public override void StartAiming(float startPitchAngle, float startYawAngle)
    {
        _isAiming = true;
        _pitchAngle = startPitchAngle;
        _yawAngle = startYawAngle;
        _cannonAim.gameObject.SetActive(true);
    }

    public override void EndAiming()
    {
        _isAiming = false;
        _cannonAim.gameObject.SetActive(false);
    }

    public void OnLoadCannonBall(CannonBallData cannonBallData)
    {
        if (_curBallCount.CurrentValue >= _cannonData.LoadMaxCount)
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
        if (_curBallCount.CurrentValue >= _cannonData.LoadMaxCount || _remainingBallCount <= 0)
        {
            return;
        }
        _targetLoadTime = _cannonData.LoadTime * _remainingBallCount / _cannonData.LoadMaxCount;
        _isFireLoading = true;
    }
    
    protected override void InitSkin()
    {
        if (_cannonSkin)
        {
            Destroy(_cannonSkin);
        }
        Skin skin = GetSkin();
        if (skin is CannonSkin cannonSkin)
        {
            if (IsSkinPrefab())
            {
                _cannonSkin = Instantiate(cannonSkin, transform);
            }
            else
            {
                _cannonSkin = cannonSkin;
            }
        }

    }

    public override void ChangeSkin()
    {
        if (_cannonSkin)
        {
            Destroy(_cannonSkin);
        }

        Skin skinPrefab = GetSkin();
        if (skinPrefab != null)
        {
            if (skinPrefab is CannonSkin cannonSkinPrefab)
            {
                 _cannonSkin = Instantiate(cannonSkinPrefab, transform);
            }
        }
    }

    /// <summary>
    /// hitPoint로 향하는 Yaw, Pitch 구하는 함수
    /// </summary>
    public override bool TryGetYawPitch( Vector3 hitPoint, out float yawDeg, out float pitchDeg)
    {
        yawDeg = 0f;
        pitchDeg = 0f;

        var fp = _cannonSkin.FirePoint;
        Vector3 toTarget = hitPoint - fp.position;
        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        // 1) "바라보는 회전"을 계산 (대입 X)
        Quaternion lookWorld = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        // 2) fp 기준 로컬 회전으로 변환
        Quaternion lookLocal = Quaternion.Inverse(fp.rotation) * lookWorld;

        // 3) 로컬 yaw/pitch 추출
        Vector3 e = lookLocal.eulerAngles;
        pitchDeg = Mathf.Repeat(e.x + 180f, 360f) - 180f;
        yawDeg   = Mathf.Repeat(e.y + 180f, 360f) - 180f;

        pitchDeg = -pitchDeg;
        return true;
    }
    
    public Vector3 GetLaunchVelocity()
    {
        var fp = _cannonSkin.FirePoint;

        // yaw/pitch는 fp 로컬 기준 회전 (원래 너가 쓰던 컨벤션 유지)
        Quaternion localAim = Quaternion.Euler(_pitchAngle, _yawAngle, 0f);

        // fp의 현재 회전을 기준으로 월드 방향 생성
        Vector3 worldDir = fp.rotation * (localAim * Vector3.forward);

        return worldDir.normalized * _cannonData.LaunchSpeed;
    }
    private void ShowAiming()
    {
        if(!_cannonAim)
        {
            return;
        }
        Vector3 position = _cannonSkin.FirePoint.position;
        Vector3 velocity = GetLaunchVelocity();
        
        _cannonAim.InitLine(_maxSteps);
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
                _hitPoint = hit.point;
                _hitPoint.y += 0.3f;
                _cannonAim.ClearEmptyLine();
                break;
            }
            _cannonAim.SetLinePosition(i, position);
        }
        //해당 Aim오브젝트는 Local, World 축 차이가 없기 때문에 world기준으로 구하기
        _surfaceAngleFinder.TryGetWorldSurfaceAngle(out Vector3 angleVec, _cannonAim.transform);
        _cannonAim.transform.eulerAngles = angleVec;
        _cannonAim.transform.position = _hitPoint;
    }

    public bool IsFireLoading() => _isFireLoading;
    private bool HasNotCannonBall() => _pool == null || _curBallCount.CurrentValue <= 0;
    public bool IsAiming() => _isAiming;
    public CannonData CannonData => _cannonData;
    public CannonSkin CannonSkin => _cannonSkin;
    public int CurBallCount => _curBallCount.CurrentValue;
    public int RemainingBallCount => _remainingBallCount;
    public int CurBallLoadCount => _curBallLoadCount;
    public int RemainingBallLoadCount => _remainingBallLoadCount;
}
