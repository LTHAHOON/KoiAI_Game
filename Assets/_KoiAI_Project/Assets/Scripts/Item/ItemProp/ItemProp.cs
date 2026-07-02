using System;

using UnityEngine;

namespace KoiAI.ItemProp
{
    using KoiAI.Audio;
    using KoiAI.Player;
    using KoiAI.Utilities;
    using KoiAI.Item;
    using NaughtyAttributes;

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
        [Header("비교 방향: CurXX (==) CompareXX")]
        [InfoBox("주체의 크기와 비교할 Pick Up 크기")]
        [SerializeField]
        private CompareEnumCondition<ItemPickUpSize> _itemSizeCompareCondition;
        [InfoBox("현재 HP비율과 비교 할 HP비율")]
        [SerializeField]
        private CompareValueCondition<float> _hpCompareCondition;
        [InfoBox("현재 슬롯 갯수와 비교 할 최대 슬롯 갯수")]
        [SerializeField]
        private CompareValueCondition<int> _slotCountCompareCondition;

        public bool CheckCanPickUp(ItemPickUpSize ownerSize = ItemPickUpSize.None, float curHPRatio = -1, int curSlotCount = -1)
        {
            bool bValidOwnerSize =  ownerSize.CompareEnumWithCondition<ItemPickUpSize, int>(_itemSizeCompareCondition);
            bool bValidHPRatio = curHPRatio.CompareWithCondition(_hpCompareCondition);
            bool bValidSlotCount = curSlotCount.CompareWithCondition(_slotCountCompareCondition);

            if(bValidOwnerSize && bValidOwnerSize && bValidSlotCount)
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
    
        public ItemPickUpEvent(ItemData itemData, AudioData itemAudioData)
        {
            _itemData = itemData;
            _itemAudioData = itemAudioData;
        }
    
        public ItemData ItemData => _itemData;
        public AudioData ItemAudioData => _itemAudioData;
    }

    [RequireComponent (typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ItemProp : MonoBehaviour
    {
        [SerializeField]
        private ItemPickUpEvent _itemPickUpEvent;
        [SerializeField]
        private GameTagName _itemOwnerTag;
        [SerializeField]
        ItemPickUpCondition itemPickUpCondition;

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
                    interactable.Interact(_itemPickUpEvent);
                    Destroy(gameObject);
                }
            }
        }
    }
}