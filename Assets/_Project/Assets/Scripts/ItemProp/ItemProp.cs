using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using R3;

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
        _meshRenderer.material = itemData.ItemMaterial;
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
