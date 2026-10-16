using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.Item;
    
    public interface IEquipmentProvider : IItemPickUpConditionProvider
    {
        public int GetRemainingSlotCount();
        public int GetMaxSlotCount();
        public ItemPickUpSize ItemPickUpSize { get; }
    }
}
