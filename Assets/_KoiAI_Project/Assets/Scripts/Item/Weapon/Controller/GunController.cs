using System;
using UnityEngine;
using static KoiAI.Player.PlayerSFXAudioFeature;

namespace KoiAI.Item
{
    using KoiAI.Pool;
    using KoiAI.Skin;
    using KoiAI.Utilities;
    using KoiAI.Audio;

    public class GunController : WeaponControllerBase
    {
        [SerializeField]
        private LayerMask _targetLayerMask;
        [SerializeField]
        private bool _useFirePointForShot;

        private GunData _gunData;
        private GunSkin _gunSkin;
        private Pool<BulletItem> _pool;

        private int _curBallCount = 0;
        private AudioSFXTarget _attackAuidoTarget;
        private int _curBallLoadCount;
        private int _remainingBallCount = 0;
        private int _remainingBallLoadCount = 0;
        private float _curLoadTime = 0f;
        private float _targetLoadTime = 0f;
        private bool _isFireLoading = false;
        private int _reloadBallCount;
        private float _pitchAngle;
        private float _yawAngle;
        private Action OnPlayFireEffect;

        private void Update()
        {
            if (!_gunData || !IsFireLoading())
            {
                return;
            }
            #region 발사체 장전 로직
            _curLoadTime += Time.deltaTime;
            float progress = _targetLoadTime > 0f ? Mathf.Clamp01(_curLoadTime / _targetLoadTime) : 1f;
            int count = Mathf.FloorToInt(_reloadBallCount * progress);
            _curBallLoadCount = _curBallCount + count;
            _remainingBallLoadCount = _gunData.IsInfiniteLoad ? _remainingBallCount : _remainingBallCount - count;
            if (progress >= 1f)
            {
                CompleteReload();
            }
            #endregion
        }

        private void OnDisable()
        {
            CancelReload();
        }

        public override void Init(WeaponBase weaponItem)
        {
            _gunData = GetWeaponData<GunData>();
            CancelReload();
            _curBallCount = _gunData.IsInfiniteLoad ? Mathf.Max(0, _gunData.LoadMaxCount) : 0;
            _remainingBallCount = 0;
            _curBallLoadCount = _curBallCount;
            _remainingBallLoadCount = 0;
            OnPlayFireEffect = () => PlayFireEffect(weaponItem);
            BulletData bulletData = _gunData.BulletData;
            ulong id = gameObject.GetEntityULongID();
            BulletItem projectilePrefab = (BulletItem)bulletData.ItemPrefab;
            PoolManager.Instance.AddPool<BulletItem>(id, projectilePrefab, bulletData.BulletPoolSize, PoolName.Projectile);
            PoolManager.Instance.TryGetPool<BulletItem>(id, out _pool);
            BulletItem[] bullets = _pool.GetAllInstanceArray();
            for (int i = 0; i < bullets.Length; i++)
            {
                //발사체 스킨 생성
                bullets[i].SetupController();
            }
            InitSkin();
        }
        
        private void PlayFireEffect(WeaponBase weaponItem)
        {
            if (_gunSkin.FirePT)
            {
                _gunSkin.FirePT.Play();
            }
            if (_gunSkin.FireAudioData)
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
                AudioManager.Instance.PlaySFX(_attackAuidoTarget, _gunSkin.FireAudioData, _gunSkin.FirePoint.position);
            }
        }

        public override bool Activate()
        {
            return ActivateWithSpread(0f);
        }

        public override bool ActivateWithSpread(float spreadAngle)
        {
            if (!_gunData || _isFireLoading)
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

            BulletItem bulletItem = _pool.Pop();
            if (!bulletItem)
            {
                return false;
            }
            --_curBallCount;
            
            if (bulletItem.IsEmptyController())
            {
                bulletItem.SetupController();
            }
            OnPlayFireEffect.Invoke();

            Transform shotOrigin = (_useFirePointForShot && _gunSkin && _gunSkin.FirePoint) ? _gunSkin.FirePoint : transform;
            Vector3 shotDirection = shotOrigin.rotation * (Quaternion.Euler(_pitchAngle, _yawAngle, 0f) * Vector3.forward);
            shotDirection = ApplySpread(shotDirection, shotOrigin.up, spreadAngle);
            if(Physics.Raycast(shotOrigin.position, shotDirection, out RaycastHit hit, 500f, _targetLayerMask))
            {
                bulletItem.BulletController.OnHit(hit);
            }
            
            PoolManager.Instance.ReturnDelay(_pool, bulletItem, 2f);

            if (_curBallCount <= 0)
            {
                TryAutoReload();
            }

            return true;
        }
        public override void SetAim(Vector2 aim)
        {
            if (!_gunData)
            {
                return;
            }
            _yawAngle = Mathf.Clamp(_yawAngle + aim.x * Time.deltaTime * 10f, _gunData.MinYawAngle, _gunData.MaxYawAngle);
            _pitchAngle = Mathf.Clamp(_pitchAngle - aim.y * Time.deltaTime * 10f, _gunData.MinPitchAngle, _gunData.MaxPitchAngle);
        }

        public override void StartAiming(float startPitchAngle = 0, float startYawAngle = 0)
        {
            _isAiming = true;
            _pitchAngle = startPitchAngle;
            _yawAngle = startYawAngle;
        }

        public override void EndAiming()
        {
            _isAiming = false;
        }

        public void OnLoadCannonBall(BulletData bulletData)
        {
            if (!_gunData || !bulletData)
            {
                return;
            }
            if (!_gunData.IsInfiniteLoad)
            {
                _remainingBallCount += bulletData.ItemCount;
            }
            TryAutoReload();
        }

        private void TryAutoReload()
        {
            if (_gunData && _gunData.IsAutoLoad && _curBallCount <= 0)
            {
                OnReLoadBullet();
            }
        }

        public void OnReLoadBullet()
        {
            //중복 장전 차단
            if (!_gunData || _isFireLoading)
            {
                return;
            }
            //탄이 꽉 차있거나 남은 탄이 없을 경우 리턴
            int missingCount = Mathf.Max(0, _gunData.LoadMaxCount - _curBallCount);
            if (missingCount == 0 || (!_gunData.IsInfiniteLoad && _remainingBallCount <= 0))
            {
                return;
            }
            _reloadBallCount = _gunData.IsInfiniteLoad ? missingCount : Mathf.Min(missingCount, _remainingBallCount);
            _curLoadTime = 0f;
            // 빈 탄창을 채우는 속도를 기준으로 보충할 탄 수만큼 장전합니다.
            _targetLoadTime = Mathf.Max(0f, _gunData.LoadTime) * _reloadBallCount / _gunData.LoadMaxCount;
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
            if (!_gunData.IsInfiniteLoad)
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

        protected override void InitSkin()
        {
            if (_gunSkin)
            {
                Destroy(_gunSkin);
            }
            var skin = GetSkin();
            if (skin is GunSkin gunSkin)
            {
                if (IsSkinPrefab())
                {
                    _gunSkin = Instantiate(gunSkin, transform);
                }
                else
                {
                    _gunSkin = gunSkin;
                }
            }

        }

        public override void ChangeSkin()
        {
            if (_gunSkin)
            {
                Destroy(_gunSkin);
            }

            var skinPrefab = GetSkin();
            if (skinPrefab != null)
            {
                if (skinPrefab is GunSkin gunSkinPrefab)
                {
                    _gunSkin = Instantiate(gunSkinPrefab, transform);
                }
            }
        }

        public override bool IsFireLoading() => _isFireLoading;
        public GunData GunData => _gunData ? _gunData : GetWeaponData<GunData>();
        public int CurBallCount => _curBallCount;
        public int RemainingBallCount => _remainingBallCount;
        public int CurBallLoadCount => _curBallLoadCount;
        public int RemainingBallLoadCount => _remainingBallLoadCount;
    }
}
