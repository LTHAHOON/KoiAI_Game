
using System;
using UnityEngine;
using static KoiAI.Player.PlayerSFXAudioFeature;

namespace KoiAI.Item
{
    using KoiAI.Skin;
    using KoiAI.Audio;
    using KoiAI.Pool;
    using KoiAI.Utilities;
    
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

        private int _curBallCount = 0;
        private AudioSFXTarget _attackAuidoTarget;
        private int _curBallLoadCount;
        private int _remainingBallCount = 0;
        private int _remainingBallLoadCount = 0;
        private float _curLoadTime = 0f;
        private float _targetLoadTime = 0f;
        private bool _isFireLoading = false;
        private int _reloadBallCount;
        private Action OnPlayFireEffect;

        private void Update()
        {
            if (!_cannonData)
            {
                return;
            }
            if (IsFireLoading())
            {
                #region 발사체 장전 로직
                _curLoadTime += Time.deltaTime;
                float progress = _targetLoadTime > 0f ? Mathf.Clamp01(_curLoadTime / _targetLoadTime) : 1f;
                int count = Mathf.FloorToInt(_reloadBallCount * progress);
                _curBallLoadCount = _curBallCount + count;
                _remainingBallLoadCount = _cannonData.IsInfiniteLoad ? _remainingBallCount : _remainingBallCount - count;
                if (progress >= 1f)
                {
                    CompleteReload();
                }
                #endregion
            }
            if (_cannonSkin && IsAiming())
            {
                ShowAiming();
            }
        }

        public override void Init(WeaponBase wepaonItem)
        {
            CancelReload();
            _curBallCount = _cannonData.IsInfiniteLoad ? Mathf.Max(0, _cannonData.LoadMaxCount) : 0;
            _remainingBallCount = 0;
            _curBallLoadCount = _curBallCount;
            _remainingBallLoadCount = 0;
            OnPlayFireEffect = () => PlayFireEffect(wepaonItem);
            CannonBallData cannonBallData = _cannonData.CannonBallData;
            ulong id =  gameObject.GetEntityULongID();
            CannonBallItem projectilePrefab = (CannonBallItem)cannonBallData.ItemPrefab;
            PoolManager.Instance.AddPool<CannonBallItem>(id, projectilePrefab, cannonBallData.CannonBallPoolSize, PoolName.Projectile);
            PoolManager.Instance.TryGetPool<CannonBallItem>(id, out _pool);
            CannonBallItem[] cannonBalls = _pool.GetAllInstanceArray();
            for (int i = 0; i < cannonBalls.Length; i++)
            {
                //발사체 스킨 생성
                cannonBalls[i].SetupController(_targetLayerMask);
            }
            _cannonAim = Instantiate(_cannonData.AimPrefab, transform);
            if (_cannonAim)
            {
                _cannonAim.gameObject.SetActive(false);
            }
            _surfaceAngleFinder = new(5);
            InitSkin();
        }

        private void PlayFireEffect(WeaponBase weaponItem)
        {
            if (_cannonSkin.FirePT)
            {
                _cannonSkin.FirePT.Play();
            }
            if (_cannonSkin.FireAudioData)
            {
                var owner = weaponItem ? weaponItem.ItemOwner : null;
                if (!owner)
                {
                    return;
                }
                if (_attackAuidoTarget == null)
                {
                    _attackAuidoTarget = owner.GetAudioSFXTarget(PlayerSFXAuidoProperty.Attack);
                }
                AudioManager.Instance.PlaySFX(_attackAuidoTarget, _cannonSkin.FireAudioData, _cannonSkin.FirePoint.position);
            }
        }

        public override bool Activate()
        {
            return ActivateWithSpread(0f);
        }

        public override bool ActivateWithSpread(float spreadAngle)
        {
            if (!_cannonData || _isFireLoading)
            {
                return false;
            }
            if (_curBallCount <= 0)
            {
                TryAutoReload();
                return false;
            }
            if (_pool == null)
            {
                return false;
            }

            CannonBallItem cannonballItem = _pool.Pop();
            if(!cannonballItem)
            {
                return false;
            }
            if (cannonballItem.IsEmptyController())
            {
                cannonballItem.SetupController(_targetLayerMask);
            }
            cannonballItem.TrailRenderer.Clear();
            cannonballItem.TrailRenderer.enabled = false;
            cannonballItem.TrailRenderer.enabled = true;
            Rigidbody bulletRigid = cannonballItem.Rigidbody;
            if (!bulletRigid)
            {
                return false;
            }
            --_curBallCount;
            OnPlayFireEffect.Invoke();
            bulletRigid.transform.position = _cannonSkin.FirePoint.position;
            bulletRigid.linearDamping = _cannonData.LinearDamping;
            bulletRigid.useGravity = true;
            Vector3 launchVelocity = GetLaunchVelocity();
            Vector3 launchDirection = ApplySpread(launchVelocity.normalized, _cannonSkin.FirePoint.up, spreadAngle);
            bulletRigid.linearVelocity = launchDirection * _cannonData.LaunchSpeed;
            PoolManager.Instance.ReturnDelay(_pool, cannonballItem, _cannonData.CannonBallData.CannonBallLifeTime);
            TryAutoReload();
            return true;
        }
        public override void SetAim(Vector2 aim)
        {
            _yawAngle += aim.x * Time.deltaTime * 10f;
            _yawAngle = Mathf.Clamp(_yawAngle, _cannonData.MinYawAngle, _cannonData.MaxYawAngle);
            _pitchAngle += -aim.y * Time.deltaTime * 10f;
            _pitchAngle = Mathf.Clamp(_pitchAngle, _cannonData.MinPitchAngle, _cannonData.MaxPitchAngle);
        }

        public override void StartAiming(float startPitchAngle = 0, float startYawAngle = 0)
        {
            _isAiming = true;
            _pitchAngle = startPitchAngle;
            _yawAngle = startYawAngle;
            if (_cannonAim)
            {
                _cannonAim.gameObject.SetActive(true);
            }
        }

        public override void EndAiming()
        {
            _isAiming = false;
            if (_cannonAim)
            {
                _cannonAim.gameObject.SetActive(false);
            }
        }

        public void OnLoadCannonBall(CannonBallData cannonBallData)
        {
            if (!_cannonData || !cannonBallData)
            {
                return;
            }
            if (!_cannonData.IsInfiniteLoad)
            {
                _remainingBallCount += cannonBallData.ItemCount;
            }
            TryAutoReload();
        }

        private void TryAutoReload()
        {
            if (_cannonData && _cannonData.IsAutoLoad && _curBallCount <= 0)
            {
                OnReLoadCannonBall();
            }
        }

        public void OnReLoadCannonBall()
        {
            //중복 장전 차단
            if (!_cannonData || _isFireLoading)
            {
                return;
            }
            //탄이 꽉 차있거나 남은 탄이 없을 경우 리턴
            int missingCount = Mathf.Max(0, _cannonData.LoadMaxCount - _curBallCount);
            if (missingCount == 0 || (!_cannonData.IsInfiniteLoad && _remainingBallCount <= 0))
            {
                return;
            }
            _reloadBallCount = _cannonData.IsInfiniteLoad ? missingCount : Mathf.Min(missingCount, _remainingBallCount);
            _curLoadTime = 0f;
            // 빈 탄창을 채우는 속도를 기준으로 보충할 탄 수만큼 장전합니다.
            _targetLoadTime = Mathf.Max(0f, _cannonData.LoadTime) * _reloadBallCount / _cannonData.LoadMaxCount;
            _curBallLoadCount = _curBallCount;
            _remainingBallLoadCount = _remainingBallCount;
            _isFireLoading = true;
            NotifyReloadStateChanged(true);
            if (_targetLoadTime <= 0f)
            {
                CompleteReload();
            }
        }

        public override bool TryInterruptReload()
        {
            if (!_isFireLoading)
            {
                return false;
            }
            // 공격 입력으로 중단할 때는 지금까지 장전된 탄을 유지합니다.
            _curBallCount = _curBallLoadCount;
            _remainingBallCount = _remainingBallLoadCount;
            CancelReload();
            return true;
        }

        private void CompleteReload()
        {
            _curBallCount += _reloadBallCount;
            if (!_cannonData.IsInfiniteLoad)
            {
                _remainingBallCount -= _reloadBallCount;
            }
            CancelReload();
        }

        private void CancelReload()
        {
            bool wasReloading = _isFireLoading;
            _isFireLoading = false;
            _reloadBallCount = 0;
            _curLoadTime = 0f;
            _targetLoadTime = 0f;
            _curBallLoadCount = _curBallCount;
            _remainingBallLoadCount = _remainingBallCount;
            if (wasReloading)
            {
                NotifyReloadStateChanged(false);
            }
        }

        private void OnDisable()
        {
            CancelReload();
        }

        protected override void InitSkin()
        {
            if (_cannonSkin)
            {
                Destroy(_cannonSkin);
            }
            var skin = GetSkin();
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

            var skinPrefab = GetSkin();
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

            //바라보는 회전을 계산
            Quaternion lookWorld = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

            //기준 로컬 회전
            Quaternion lookLocal = Quaternion.Inverse(fp.rotation) * lookWorld;

            //로컬 yaw/pitch 추출
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
                velocity += UnityEngine.Physics.gravity * _timeStep;
                position += velocity * _timeStep;
                Vector3 move = position - previousPosition;
                float distance = move.magnitude;

                if (UnityEngine.Physics.Raycast(previousPosition, move.normalized, out RaycastHit hit, distance, _cannonData.LayerMaskForAim))
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

        public override bool IsFireLoading() => _isFireLoading;
        public CannonData CannonData => _cannonData;
        public int CurBallCount => _curBallCount;
        public int RemainingBallCount => _remainingBallCount;
        public int CurBallLoadCount => _curBallLoadCount;
        public int RemainingBallLoadCount => _remainingBallLoadCount;
    }
}
