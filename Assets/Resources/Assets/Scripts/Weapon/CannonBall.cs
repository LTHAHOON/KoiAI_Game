using UnityEngine;

public class CannonBall : ResourceBase
{
    [SerializeField]
    private CannonBallData _cannonBallData;
    [SerializeField]
    private Rigidbody _rigid;
    [SerializeField]
    private TrailRenderer _trailRenderer;
    
    private PlayerEquipment _equipmentFeature;
    public Rigidbody Rigidbody => _rigid;
    public TrailRenderer TrailRenderer => _trailRenderer;

    public override ItemData GetItemData()
    {
        return _cannonBallData;
    }

    public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
    {
        base.Init(itemOwner, itemUI, curSlotType);
        #region PlayerEquipment 참조
        _equipmentFeature = (PlayerEquipment)ItemOwner.GetPlayerFeatureWithProperty(PlayerFeature.PlayerFeatureProperty.Equipment);
        #endregion
        _equipmentFeature.SetItemCountOfNotEquipped(this, _cannonBallData.ProjectileCount);
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
        if (weaponItem.TryGetItemChildClass<CannonController>(out CannonController cannonController))
        {
            CannonData cannonData = cannonController.GetCannonData();
            //ID와 타입이 같은 지 체크
            if(cannonData.CannonBallData.ProjectileType == _cannonBallData.ProjectileType
                && cannonData.CannonBallData.ItemId == _cannonBallData.ItemId)
            {
                cannonController.OnLoadCannonBall(_cannonBallData);
                ItemSlotType curSlotType = GetCurrentSlotType();
                _equipmentFeature.RemoveItemInSlot(this, curSlotType);
            }
        }

    }
}
