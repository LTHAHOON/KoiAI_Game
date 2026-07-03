using KoiAI.ItemProp;
using UnityEngine;

namespace KoiAI.Interact
{
    public interface IItemPickUpConditionProvider
    {
        public void RefreshItemPickUpCondition(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition);
    }
}
