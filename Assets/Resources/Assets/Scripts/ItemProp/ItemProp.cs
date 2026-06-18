using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[RequireComponent (typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ItemProp : MonoBehaviour
{
    [SerializeField]
    private ItemData _itemData;
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
        if(_itemData == null || _meshFilter.mesh == null || _meshRenderer.material == null)
        {
            return;
        }
        _meshFilter.mesh = _itemData.ItemMesh;
        _meshRenderer.material = _itemData.ItemMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(_ownerTag))
        {
            
        }
    }
}
