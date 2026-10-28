using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Player;
    using KoiAI.Skin;

    [RequireComponent(typeof(CannonBallController))]
    public class CannonBallItem : ResourceBase
    {
        [SerializeField]
        private CannonBallData _cannonBallData;
        [SerializeField]
        private CannonBallController _cannonBallController;

        private PlayerEquipment _equipmentFeature;
        private CannonBallSkin _cannonBallSkin;
        private bool _isControllerInitialized;

        public override void SetItemData(ItemData itemData)
        {
            base.SetItemData(itemData);
            _cannonBallData = itemData as CannonBallData;
        }

        public override ItemData GetItemData()
        {
            return InitializedItemData ?? _cannonBallData;
        }

        public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            base.Init(itemOwner, itemUI, curSlotType);
            #region PlayerEquipment 참조
            _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
            #endregion

        }

        public bool SetupController(LayerMask targetLayerMask)
        {
            if (_isControllerInitialized || !_cannonBallController || !_cannonBallData || !_cannonBallData.CannonBallSkin)
            {
                if (!_cannonBallData)
                {
                    Debug.LogError($"{nameof(CannonBallItem)} has no {nameof(CannonBallData)} assigned.", this);
                }
                return _isControllerInitialized;
            }

            _cannonBallSkin = Instantiate(_cannonBallData.CannonBallSkin, transform);
            _cannonBallController.SetSkin(_cannonBallSkin);
            _isControllerInitialized = _cannonBallController.Init(_cannonBallData, targetLayerMask);
            if (!_isControllerInitialized)
            {
                Destroy(_cannonBallSkin.gameObject);
                _cannonBallSkin = null;
            }
            return _isControllerInitialized;
        }

        public override void UseItem()
        {
            if (!_equipmentFeature)
            {
                return;
            }
            //장착된 슬롯(무기) 가져오기
            Slot weaponSlot = _equipmentFeature.GetSelectedSlot(ItemSlotType.Equipped);
            //해당 슬롯에 있는 아이템(무기) 가져오기
            ItemBase weaponItem = weaponSlot.GetItem();
            if (weaponItem == null)
            {
                return;
            }
            if (weaponItem.TryGetItemChildClass<CannonItem>(out CannonItem cannonItem))
            {
                CannonData cannonData = cannonItem.GetCannonData();
                //같은 포탄 프리팹을 사용하는 데이터면 수량이 달라도 장전할 수 있습니다.
                if(cannonData.CannonBallData.ProjectileType == _cannonBallData.ProjectileType
                   && cannonData.CannonBallData.ItemPrefab == _cannonBallData.ItemPrefab)
                {
                    cannonItem.OnLoadCannonBall(_cannonBallData);
                    ItemSlotType curSlotType = GetCurrentSlotType();
                    _equipmentFeature.RemoveItemInSlot(this);
                }
            }

        }

        public bool IsEmptyController() => !_isControllerInitialized;

        public Rigidbody Rigidbody => _cannonBallController.GetCannonBallSkin().Rigidbody;
        public TrailRenderer TrailRenderer => _cannonBallController.GetCannonBallSkin().TrailRenderer;
    }
}
