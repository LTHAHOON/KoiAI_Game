using KoiAI.Audio;
using KoiAI.ItemProp;
using R3;
using UnityEngine;

namespace KoiAI.Player
{
    public class ItemInteractable : BaseInteractable<ItemPickUpEvent>
    {
        [SerializeField]
        private AudioSFXTarget _mainSFXTarget;
    
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
    }
}
