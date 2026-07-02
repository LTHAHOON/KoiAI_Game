using System;
using KoiAI.Audio;
using KoiAI.ItemProp;
using NaughtyAttributes;
using R3;
using UnityEngine;

namespace KoiAI.Player
{
    public class ItemInteractable : BaseInteractable<ItemPickUpEvent>
    {
        [SerializeField]
        [BoxGroup("오직 데이터만 필요")]
        private ItemPickUpCondition _itemPickUpConditionData;
        [SerializeField]
        private AudioSFXTarget _mainSFXTarget;

        public Action<ItemPickUpCondition> OnRefreshItemConditionData { get; set; }

        private void Awake()
        {
            //아이템 흭득 소리 및 파티클 등등 연결
            OnInteract
                .Subscribe(itemPickUpEvent =>
                {
                    AudioManager.Instance.PlaySFX(_mainSFXTarget, itemPickUpEvent.ItemAudioData, transform.position);
                }).AddTo(this);

        }
    
        public override void Interact(ItemPickUpEvent dataEvent)
        {
            base.Interact(dataEvent);
        }
        
        public ItemPickUpCondition GetItemPickUpConditionData() => _itemPickUpConditionData;
    }
}
