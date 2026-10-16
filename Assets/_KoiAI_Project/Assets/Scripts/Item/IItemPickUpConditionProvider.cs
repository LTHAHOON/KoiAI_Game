using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.Item;

    public interface IItemPickUpConditionProvider
    {
        public void RefreshItemPickUpCondition(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition);
    }
}
