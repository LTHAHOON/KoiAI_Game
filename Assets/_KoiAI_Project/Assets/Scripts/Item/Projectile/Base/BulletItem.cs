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

        public override void SetItemData(ItemData itemData)
        {
            base.SetItemData(itemData);
            _bulletData = itemData as BulletData;
        }

        public override ItemData GetItemData()
        {
            return InitializedItemData ?? _bulletData;
        }

        public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            base.Init(itemOwner, itemUI, curSlotType);
            #region PlayerEquipment 참조
            _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
            #endregion

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
                //같은 탄환 프리팹을 사용하는 데이터면 수량이 달라도 장전할 수 있습니다.
                if (gunData.BulletData.ProjectileType == _bulletData.ProjectileType
                   && gunData.BulletData.ItemPrefab == _bulletData.ItemPrefab)
                {
                    gundItem.OnLoadCannonBall(_bulletData);
                    _equipmentFeature.RemoveItemInSlot(this);
                }
            }

        }

        public BulletController BulletController => _bulletController;
        public bool IsEmptyController() => _bulletController == null;

    }
}
