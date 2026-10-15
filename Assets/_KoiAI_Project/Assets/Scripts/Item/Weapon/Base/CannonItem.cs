using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Item
{
    using KoiAI.Input;
    using KoiAI.Player;
    
    [RequireComponent(typeof(CannonController))]
    public class CannonItem : WeaponBase
    {
        private CannonController _cannonControl;
        private PlayerEquipment _equipmentFeature;
        private PlayerRotation _rotationFeature;
        private CannonData _cannonData;
        private Vector2 _aim;
        private bool _isControllerInitialized;

        private bool IsEquipped => isActiveAndEnabled && GetCurrentSlotType() == ItemSlotType.Equipped;

        private void Awake()
        {
            _cannonControl = GetComponent<CannonController>();
            _cannonData = _cannonControl.CannonData;
        }

        private void Update()
        {
            if(!_cannonControl || !_cannonData)
            {
                return;
            }
            if(_cannonControl.IsAiming())
            {
                _cannonControl.SetAim(_aim);
            }

            //장전 중일 경우 무기 정보(탄 갯수 Text) 설정
            if (_equipmentFeature && IsEquipped && _cannonControl.IsFireLoading())
            {
                _equipmentFeature.SetWeaponInfo(_cannonControl.CurBallLoadCount, _cannonControl.RemainingBallLoadCount);
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
            bool wasAiming = _cannonControl && _cannonControl.IsAiming();
            if (_cannonControl)
            {
                _cannonControl.EndAiming();
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
            if (_cannonControl)
            {
                _cannonControl.OnReloadStateChanged -= OnReloadAmmoChanged;
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
            if (!_cannonControl)
            {
                _cannonControl = GetComponent<CannonController>();
            }
            _cannonData = _cannonControl.CannonData;
            _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
            _cannonControl.OnReloadStateChanged -= OnReloadAmmoChanged;
            _cannonControl.OnReloadStateChanged += OnReloadAmmoChanged;
            if (curSlotType == ItemSlotType.Equipped)
            {
                InitializeWeapon();
            }
        }

        private void InitializeWeapon()
        {
            if (!_isControllerInitialized)
            {
                _cannonControl.Init(this);
                _isControllerInitialized = true;
            }
            _rotationFeature = ItemOwner.GetComponent<PlayerRotation>();
            if (IsEquipped)
            {
                ConnectPlayerIA();
                _equipmentFeature.SetWeaponInfo(_cannonControl.CurBallCount, _cannonControl.RemainingBallCount);
            }
        }

        private void OnReloadAmmoChanged(bool isReloading)
        {
            if (_equipmentFeature && IsEquipped)
            {
                _equipmentFeature.SetWeaponInfo(_cannonControl.CurBallCount, _cannonControl.RemainingBallCount);
            }
        }
        public override ItemData GetItemData()
        {
            return _cannonControl.CannonData;
        }

        public CannonData GetCannonData()
        {
            return _cannonControl.CannonData;
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
            if(!bExistSameItem)
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
                _equipmentFeature.CreateAndPushItemInSlot(ItemSlotType.NotEquipped, _cannonData.CannonBallData);
            }
            #endregion

            #region Projectile Pooling
            if (!bExistSameItem)
            {
                InitializeWeapon();
            }
            #endregion

            if(bExistSameItem)
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
            if(context.performed)
            {
                float pitchAngle = (_cannonData.MinPitchAngle + _cannonData.MaxPitchAngle) / 2;
                float yawAngle = (_cannonData.MinYawAngle + _cannonData.MaxYawAngle) / 2;
                _cannonControl.StartAiming(pitchAngle, yawAngle);
                _rotationFeature.DisConnectPlayerIA();
                _rotationFeature.SetInput(new(0, 1));
            }
            if(context.canceled)
            {
                _cannonControl.EndAiming();
                _rotationFeature.ConnectPlayerIA();
            }
        }

        public void OnProjectileAiming(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized)
            {
                return;
            }
            if(!_cannonControl.IsAiming())
            {
                _aim = Vector2.zero;
                return;
            }
            if (context.performed)
            {
                _aim = context.ReadValue<Vector2>() * _cannonData.AimSensitity;
            }
            if(context.canceled)
            {
                _aim = Vector2.zero;
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (!IsEquipped || !_isControllerInitialized)
            {
                return;
            }
            if(!_cannonControl.IsAiming() || gameObject.activeSelf == false)
            {
                return;
            }
            if(context.performed)
            {
                if (_cannonControl.TryInterruptReload())
                {
                    return;
                }
                bool bScucessActivate = _cannonControl.Activate();
                if(bScucessActivate)
                {
                    int curBallCount = _cannonControl.CurBallCount;
                    int remainingCount = _cannonControl.RemainingBallCount;
                    _equipmentFeature.SetWeaponInfo(curBallCount, remainingCount);
                }
            }
        }

        /// <summary>
        /// 발사체 장전
        /// </summary>
        public void OnLoadCannonBall(CannonBallData cannonBallData)
        {
            _cannonControl.OnLoadCannonBall(cannonBallData);
            _equipmentFeature.SetWeaponInfo(_cannonControl.CurBallCount, _cannonControl.RemainingBallCount);
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
                _cannonControl.OnReLoadCannonBall();
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
