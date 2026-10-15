using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Item
{
    using KoiAI.Input;
    using KoiAI.Player;

    [RequireComponent(typeof(GunController))]
    public class GunItem : WeaponBase
    {
        private GunController _gunControl;
        private PlayerEquipment _equipmentFeature;
        private PlayerRotation _rotationFeature;
        private GunData _gunData;
        private Vector2 _aim;
        private bool _isControllerInitialized;

        private bool IsEquipped => isActiveAndEnabled && GetCurrentSlotType() == ItemSlotType.Equipped;

        private void Awake()
        {
            _gunControl = GetComponent<GunController>();
            _gunData = _gunControl.GunData;
        }

        private void Update()
        {
            if (!_gunControl || !_gunData)
            {
                return;
            }
            if (_gunControl.IsAiming())
            {
                _gunControl.SetAim(_aim);
            }

            //장전 중일 경우 무기 정보(탄 갯수 Text) 설정
            if (_equipmentFeature && IsEquipped && _gunControl.IsFireLoading())
            {
                _equipmentFeature.SetWeaponInfo(_gunControl.CurBallLoadCount, _gunControl.RemainingBallLoadCount);
            }
        }

        private void OnEnable()
        {
            if (IsEquipped && _equipmentFeature)
            {
                InitializeWeapon();
            }
        }

        private void OnDisable()
        {
            bool wasAiming = _gunControl && _gunControl.IsAiming();
            if (_gunControl)
            {
                _gunControl.EndAiming();
            }
            _aim = Vector2.zero;
            DisConnectPlayerIA();
            if (wasAiming && _rotationFeature)
            {
                _rotationFeature.ConnectPlayerIA();
            }
        }
        private void OnDestroy()
        {
            if (_gunControl)
            {
                _gunControl.OnReloadStateChanged -= OnReloadAmmoChanged;
            }
            var curSlotType = GetCurrentSlotType();
            if (curSlotType == ItemSlotType.Equipped)
            {
                DisConnectPlayerIA();
            }
        }

        /// <summary>
        /// 아이템 초기화(본체를 생성하기 전 세팅)
        /// </summary>
        public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            base.Init(itemOwner, itemUI, curSlotType);
            if (!_gunControl)
            {
                _gunControl = GetComponent<GunController>();
            }
            _gunData = _gunControl.GunData;
            _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
            _gunControl.OnReloadStateChanged -= OnReloadAmmoChanged;
            _gunControl.OnReloadStateChanged += OnReloadAmmoChanged;
            if (curSlotType == ItemSlotType.Equipped)
            {
                InitializeWeapon();
            }
        }

        private void InitializeWeapon()
        {
            if (!_isControllerInitialized)
            {
                _gunControl.Init(this);
                _isControllerInitialized = true;
            }
            _rotationFeature = ItemOwner.GetComponent<PlayerRotation>();
            if (IsEquipped)
            {
                ConnectPlayerIA();
                _equipmentFeature.SetWeaponInfo(_gunControl.CurBallCount, _gunControl.RemainingBallCount);
            }
        }

        private void OnReloadAmmoChanged(bool isReloading)
        {
            if (_equipmentFeature && IsEquipped)
            {
                _equipmentFeature.SetWeaponInfo(_gunControl.CurBallCount, _gunControl.RemainingBallCount);
            }
        }
        public override ItemData GetItemData()
        {
            return _gunControl.GunData;
        }

        public GunData GetGunData()
        {
            return _gunControl.GunData;
        }

        public override void UseItem()
        {
            if (!_equipmentFeature)
            {
                return;
            }
            //장착된 무기들중 같은 무기가 있는지 체크(있으면 장착하지 않고 파기)
            bool bExistSameItem = _equipmentFeature.IsExistSameID(this, ItemSlotType.Equipped);
            #region playerIA Setting
            if (!bExistSameItem)
            {
                _rotationFeature = (PlayerRotation)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Rotation);
                ConnectPlayerIA();
            }
            #endregion

            #region 해당 아이템 장착
            if (!bExistSameItem)
            {
                _equipmentFeature.PushItemInSlot(this, ItemSlotType.Equipped);
                _equipmentFeature.EquipItem(this);
            }
            #endregion

            #region 발사체 Item 하나 생성
            if (_equipmentFeature)
            {
                _equipmentFeature.CreateAndPushItemInSlot(ItemSlotType.NotEquipped, _gunData.BulletData);
            }
            #endregion

            #region Projectile Pooling
            if (!bExistSameItem)
            {
                InitializeWeapon();
            }
            #endregion

            if (bExistSameItem)
            {
                _equipmentFeature.RemoveItemInSlot(this);
            }
        }

        public void OnStartProjectileAiming(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized)
            {
                return;
            }
            if (context.performed)
            {
                float pitchAngle = (_gunData.MinPitchAngle + _gunData.MaxPitchAngle) / 2;
                float yawAngle = (_gunData.MinYawAngle + _gunData.MaxYawAngle) / 2;
                _gunControl.StartAiming(pitchAngle, yawAngle);
                _rotationFeature.DisConnectPlayerIA();
                _rotationFeature.SetInput(new(0, 1));
            }
            if (context.canceled)
            {
                _gunControl.EndAiming();
                _rotationFeature.ConnectPlayerIA();
            }
        }

  

        public void OnFire(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized)
            {
                return;
            }
            if (!_gunControl.IsAiming() || gameObject.activeSelf == false)
            {
                return;
            }
            if (context.performed)
            {
                if (_gunControl.TryInterruptReload())
                {
                    return;
                }
                bool bScucessActivate = _gunControl.Activate();
                if (bScucessActivate)
                {
                    int curBallCount = _gunControl.CurBallCount;
                    int remainingCount = _gunControl.RemainingBallCount;
                    _equipmentFeature.SetWeaponInfo(curBallCount, remainingCount);
                }
            }
        }

        public void OnProjectileAiming(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized || !_gunControl.IsAiming())
            {
                _aim = Vector2.zero;
                return;
            }
            if (context.performed)
            {
                _aim = context.ReadValue<Vector2>() * _gunData.AimSensitity;
            }
            if (context.canceled)
            {
                _aim = Vector2.zero;
            }
        }

        /// <summary>
        /// 발사체 장전
        /// </summary>
        public void OnLoadCannonBall(BulletData bulletData)
        {
            _gunControl.OnLoadCannonBall(bulletData);
            _equipmentFeature.SetWeaponInfo(_gunControl.CurBallCount, _gunControl.RemainingBallCount);
        }

        /// <summary>
        /// 발사체 재장전
        /// </summary>
        private void OnReLoadCannonBall(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized)
            {
                return;
            }
            if (context.performed)
            {
                _gunControl.OnReLoadCannonBall();
            }
        }

        protected override void ConnectPlayerIA()
        {
            DisConnectPlayerIA();
            if (!IsEquipped || !_isControllerInitialized || InputService.PlayerIA == null)
            {
                return;
            }
            InputService.PlayerIA.Player.Fire.performed += OnFire;
            InputService.PlayerIA.Player.FireLoad.performed += OnReLoadCannonBall;
            InputService.PlayerIA.Player.ProjectileAiming.performed += OnProjectileAiming;
            InputService.PlayerIA.Player.ProjectileAiming.canceled += OnProjectileAiming;
            InputService.PlayerIA.Player.StartProjectileAiming.performed += OnStartProjectileAiming;
            InputService.PlayerIA.Player.StartProjectileAiming.canceled += OnStartProjectileAiming;
        }

        protected override void DisConnectPlayerIA()
        {
            if (InputService.PlayerIA == null)
            {
                return;
            }
            InputService.PlayerIA.Player.Fire.performed -= OnFire;
            InputService.PlayerIA.Player.FireLoad.performed -= OnReLoadCannonBall;
            InputService.PlayerIA.Player.ProjectileAiming.performed -= OnProjectileAiming;
            InputService.PlayerIA.Player.ProjectileAiming.canceled -= OnProjectileAiming;
            InputService.PlayerIA.Player.StartProjectileAiming.performed -= OnStartProjectileAiming;
            InputService.PlayerIA.Player.StartProjectileAiming.canceled -= OnStartProjectileAiming;
        }
    }
}
