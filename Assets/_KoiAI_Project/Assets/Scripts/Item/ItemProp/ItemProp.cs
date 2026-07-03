using System;
using UnityEngine;
using NaughtyAttributes;

namespace KoiAI.ItemProp
{
    using KoiAI.Audio;
    using KoiAI.Player;
    using KoiAI.Utilities;
    using KoiAI.Item;
    using KoiAI.Interact;

    public enum ItemPickUpSize : int
    {
        None = 0,
        Small = 5,
        Middle = 10,
        Large = 15
    }

    [Serializable]
    public class ItemPickUpCondition
    {
        [Tooltip("해당 ComparisonType이 None일 경우 True 반환")]
        [Header("비교할 Pick Up 크기")]
        public CompareEnumCondition<ItemPickUpSize> itemSizeCompareCondition;
        [Tooltip("해당 ComparisonType이 None일 경우 True 반환")]
        [Header("비교할 HP비율")]
        public CompareValueCondition<float> hpCompareCondition;
        [Tooltip("해당 ComparisonType이 None일 경우 True 반환")]
        [Header("비교할 슬롯 갯수(자동으로 정해집니다.)")]
        public CompareValueCondition<int> slotCountCompareCondition;
        
        public bool CheckCanPickUp(ItemPickUpCondition currentConditionData = null)
        {
            if (currentConditionData == null)
            {
                //조건이 없을 경우 true 반환
                return true;
            }
            if(!currentConditionData.IsAllDataOnly(currentConditionData))
            {
                return false;
            }
            var curOwnerSizeData = currentConditionData.itemSizeCompareCondition;
            var curHpRatioData = currentConditionData.hpCompareCondition;
            var curSlotCountData = currentConditionData.slotCountCompareCondition;

            bool bValidOwnerSize =  curOwnerSizeData.CompareValue.CompareEnumWithCondition<ItemPickUpSize, int>(itemSizeCompareCondition);
            bool bValidHpRatio =  curHpRatioData.CompareValue.CompareWithCondition(hpCompareCondition);
            bool bValidSlotCount = curSlotCountData.CompareValue.CompareWithCondition(slotCountCompareCondition);

            if(bValidOwnerSize && bValidHpRatio && bValidSlotCount)
            {
                return true;
            }
            return false;
        }
        public bool IsAllDataOnly(ItemPickUpCondition currentConditionData) => 
            currentConditionData.itemSizeCompareCondition.IsDataOnly && 
            currentConditionData.hpCompareCondition.IsDataOnly && 
            currentConditionData.slotCountCompareCondition.IsDataOnly;
    }

    [Serializable]
    public struct ItemPickUpEvent
    {
        [SerializeField] 
        private ItemData _itemData;
        [SerializeField] 
        private AudioData _itemAudioData;
        [SerializeField]
        [Header("비교 방향: CurXX (==) CompareXX")]
        private ItemPickUpCondition _itemPickUpCondition;
    
        public ItemData ItemData => _itemData;
        public AudioData ItemAudioData => _itemAudioData;
        public ItemPickUpCondition ItemPickUpCondition => _itemPickUpCondition;
    }

    [RequireComponent (typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ItemProp : MonoBehaviour
    {
        [BoxGroup]
        [SerializeField]
        private ItemPickUpEvent _itemPickUpEvent;
        [SerializeField]
        private GameTagName _itemOwnerTag;

        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private string _ownerTag;
        private void Awake()
        {
            _ownerTag = GameTags.GetGameTag(_itemOwnerTag);
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            SetItemProp();
        }

        private void SetItemProp()
        {
            ItemData itemData = _itemPickUpEvent.ItemData;
            if(itemData == null || _meshFilter.mesh == null || _meshRenderer.material == null)
            {
                return;
            }
            _meshFilter.mesh = itemData.ItemMesh;
            _meshRenderer.materials = itemData.ItemMaterials;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(_ownerTag))
            {
                if (other.TryGetComponent(out ItemPickUpInteractable interactable))
                {
                    ItemPickUpCondition currentConditionData = interactable.GetItemPickUpConditionData();
                    ItemPickUpConditionBinder conditionBinder = interactable.GetItemPickUpConditionBinder();
                    conditionBinder.RefreshProviders(currentConditionData, _itemPickUpEvent.ItemPickUpCondition);

                    bool canPickUp = _itemPickUpEvent.ItemPickUpCondition.CheckCanPickUp(currentConditionData);
                    if (!canPickUp)
                    {
                        return;
                    }
                    interactable.Interact(_itemPickUpEvent);
                    Destroy(gameObject);
                }
            }
        }
    }
}