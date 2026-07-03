
using UnityEngine;

namespace KoiAI.Interact
{
    using KoiAI.ItemProp;

    public class ItemPickUpConditionBinder : MonoBehaviour
    {
        private IItemPickUpConditionProvider[] _conditionProviders;
        private void Awake()
        {
            _conditionProviders = GetComponents<IItemPickUpConditionProvider>();
        }

        /// <summary>
        /// 현재 아이템 픽업 조건 데이터 새로고침
        /// </summary>
        public void RefreshProviders(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition)
        {
            if(_conditionProviders == null)
            {
                return;
            }
            for (int i = 0; i < _conditionProviders.Length; i++)
            {
                _conditionProviders[i].RefreshItemPickUpCondition(currentConditionData, compareCondition);
            }
        }
    }
}
