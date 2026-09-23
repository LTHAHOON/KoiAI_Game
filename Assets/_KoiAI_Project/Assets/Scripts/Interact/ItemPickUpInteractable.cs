using System;
using R3;
using UnityEngine;
using NaughtyAttributes;

namespace KoiAI.Interact
{
    using KoiAI.Audio;
    using KoiAI.Core;
    using KoiAI.Item;
    using KoiAI.ItemProp;

    [RequireComponent(typeof(EntityIdentity))]
    public class ItemPickUpInteractable : BaseInteractable<ItemPickUpEvent>
    {
        [SerializeField]
        private ItemPickUpConditionBinder _itemPickUpConditionBinder;
        [SerializeField]
        [BoxGroup("오직 데이터만 필요")]
        private ItemPickUpCondition _itemPickUpConditionData;
        [SerializeField]
        private AudioSFXTarget _mainSFXTarget;

        private EntityIdentity _myIdentity;
        private void Awake()
        {
            _myIdentity = GetComponent<EntityIdentity>();
            //아이템 흭득 소리 및 파티클 등등 연결
            OnInteract
                .Subscribe(itemPickUpEvent =>
                {
                    AudioManager.Instance.PlaySFX(_mainSFXTarget, itemPickUpEvent.ItemAudioData, transform.position);
                     GameplayEvents.OnCollected?.Invoke(new(_myIdentity, itemPickUpEvent.ItemIdentity, 1));
                }).AddTo(this);

        }
        
        public ItemPickUpCondition GetItemPickUpConditionData() => _itemPickUpConditionData;
        public ItemPickUpConditionBinder GetItemPickUpConditionBinder() => _itemPickUpConditionBinder;
    }
}
