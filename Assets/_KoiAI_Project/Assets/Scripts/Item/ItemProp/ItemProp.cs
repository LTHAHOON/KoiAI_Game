using System;
using UnityEngine;
using NaughtyAttributes;

namespace KoiAI.ItemProp
{
    using KoiAI.Audio;
    using KoiAI.Player;
    using KoiAI.Utilities;
    using KoiAI.Item;

    public enum ItemPickUpSize : int
    {
        None = 0,
        Small = 5,
        Middle = 10,
        Large = 15
    }

    [Serializable]
    public struct ItemPickUpCondition
    {
        [Header("비교할 Pick Up 크기")]
        [SerializeField]
        private CompareEnumCondition<ItemPickUpSize> _itemSizeCompareCondition;
        [Header("비교할 HP비율")]
        [SerializeField]
        private CompareValueCondition<float> _hpCompareCondition;
        [Header("비교할 슬롯 갯수")]
        [SerializeField]
        private CompareValueCondition<int> _slotCountCompareCondition;
        
        public bool CheckCanPickUp(ItemPickUpCondition? currentConditionData = null)
        {
            if (!currentConditionData.HasValue)
            {
                //조건이 없을 경우 true 반환
                return true;
            }
            var curOwnerSizeData = currentConditionData.Value._itemSizeCompareCondition;
            var curHpRatioData = currentConditionData.Value._hpCompareCondition;
            var curSlotCountData = currentConditionData.Value._slotCountCompareCondition;
            bool bValidOwnerSize =  curOwnerSizeData.CompareValue.CompareEnumWithCondition<ItemPickUpSize, int>(_itemSizeCompareCondition);
            bool bValidHpRatio =  curHpRatioData.CompareValue.CompareWithCondition(_hpCompareCondition);
            bool bValidSlotCount = curSlotCountData.CompareValue.CompareWithCondition(_slotCountCompareCondition);

            if(bValidOwnerSize && bValidHpRatio && bValidSlotCount)
            {
                return true;
            }
            return false;
        }
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
                if (other.TryGetComponent(out ItemInteractable interactable))
                {
                    var itemPickUpConditionData = interactable.GetItemPickUpConditionData();
                    bool canPickUp = _itemPickUpEvent.ItemPickUpCondition.CheckCanPickUp(itemPickUpConditionData);
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