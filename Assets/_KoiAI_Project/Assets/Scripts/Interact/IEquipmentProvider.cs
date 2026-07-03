using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.ItemProp;
    
    public interface IEquipmentProvider : IItemPickUpConditionProvider
    {
        public int GetRemainingSlotCount();
        public int GetMaxSlotCount();
        public ItemPickUpSize ItemPickUpSize { get; }
    }
}
