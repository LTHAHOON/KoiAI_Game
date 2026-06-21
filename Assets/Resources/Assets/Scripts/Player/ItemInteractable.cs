using System;
using R3;
using UnityEditor;
using UnityEngine;

public class ItemInteractable : BaseInteractable<ItemPickUpEvent>
{
    [SerializeField]
    private AudioTarget _audioTarget;
    
    private void Awake()
    {
        //아이템 흭득 소리 및 파티클 등등 연결
        OnInteract
            .Subscribe(itemPickUpEvent =>
            {
                AudioManager.Instance.PlaySFX(_audioTarget, itemPickUpEvent.ItemAudioData, transform.position);
            });
    }
    
    public override void Interact(ItemPickUpEvent dataEvent)
    {
        base.Interact(dataEvent);
    }
}
