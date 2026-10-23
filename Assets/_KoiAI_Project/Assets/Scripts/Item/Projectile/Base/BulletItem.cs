using KoiAI.Player;
using UnityEngine;

namespace KoiAI.Item
{
    public class BulletItem : ResourceBase
    {
        [SerializeField]
        private BulletData _bulletData;

        private PlayerEquipment _equipmentFeature;
        private BulletController _bulletController;
        public override ItemData GetItemData()
        {
            return _bulletData;
        }

        public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            base.Init(itemOwner, itemUI, curSlotType);
            #region PlayerEquipment 참조
            _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
            #endregion

        }

        public override void SetItemCountInSlot()
        {
            _equipmentFeature.SetItemCount(this, _bulletData.ProjectileCount);
        }

        public void SetupController()
        {
            _bulletController = Instantiate(_bulletData.BulletController, transform);
            _bulletController.Init(_bulletData);
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
            if (weaponItem.TryGetItemChildClass<GunItem>(out GunItem gundItem))
            {
                GunData gunData = gundItem.GetGunData();
                //ID와 타입이 같은 지 체크
                if (gunData.BulletData.ProjectileType == _bulletData.ProjectileType
                   && gunData.BulletData.ItemId == _bulletData.ItemId)
                {
                    gundItem.OnLoadCannonBall(_bulletData);
                    ItemSlotType curSlotType = GetCurrentSlotType();
                    _equipmentFeature.RemoveItemInSlot(this);
                }
            }

        }

        public BulletController BulletController => _bulletController;
        public bool IsEmptyController() => _bulletController == null;

    }
}
