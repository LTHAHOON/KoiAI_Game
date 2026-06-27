using KoiAI.Inventory;
using KoiAI.Player;
using KoiAI.Projectile;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Weapon
{
    [RequireComponent(typeof(CannonController))]
    public class CannonItem : WeaponBase
    {
        private CannonController _cannonControl;
        private PlayerEquipment _equipmentFeature;
        private PlayerRotation _rotationFeature;
        private CannonData _cannonData;
        private Vector2 _aim;

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
            if (_cannonControl.IsFireLoading())
            {
                _equipmentFeature.SetWeaponInfo(_cannonControl.CurBallLoadCount, _cannonControl.RemainingBallLoadCount);
            }
        }

        private void OnEnable()
        {
            var curSlotType = GetCurrentSlotType();
            if (curSlotType == ItemSlotType.Equipped)
            {
                int curBallCount = _cannonControl.CurBallCount;
                int remainingBallCount = _cannonControl.RemainingBallCount;
                _equipmentFeature.SetWeaponInfo(curBallCount, remainingBallCount);
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
                ConnectPlayerIA(ItemOwner.PlayerIA);
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
                _cannonControl.Init(this);
            }
            #endregion

            if(bExistSameItem)
            {
                _equipmentFeature.RemoveItemInSlot(this);
            }
        }

        public void OnStartProjectileAiming(InputAction.CallbackContext context)
        {
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
            if(!_cannonControl.IsAiming() || gameObject.activeSelf == false)
            {
                return;
            }
            if(context.performed)
            {
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
            if (context.performed)
            {
                _cannonControl.OnReLoadCannonBall();
            }
        }

        protected override void ConnectPlayerIA(PlayerInputAction playerIA)
        {
            playerIA.Player.Fire.performed += OnFire;
            playerIA.Player.FireLoad.performed += OnReLoadCannonBall;
            playerIA.Player.ProjectileAiming.performed += OnProjectileAiming;
            playerIA.Player.ProjectileAiming.canceled += OnProjectileAiming;
            playerIA.Player.StartProjectileAiming.performed += OnStartProjectileAiming;
            playerIA.Player.StartProjectileAiming.canceled += OnStartProjectileAiming;
        }

        protected override void DisConnectPlayerIA(PlayerInputAction playerIA)
        {
            playerIA.Player.Fire.performed -= OnFire;
            playerIA.Player.FireLoad.performed -= OnReLoadCannonBall;
            playerIA.Player.ProjectileAiming.performed -= OnProjectileAiming;
            playerIA.Player.ProjectileAiming.canceled -= OnProjectileAiming;
            playerIA.Player.StartProjectileAiming.performed -= OnStartProjectileAiming;
            playerIA.Player.StartProjectileAiming.canceled -= OnStartProjectileAiming;
        }
    }
}
