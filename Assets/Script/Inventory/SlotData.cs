using UnityEngine;

[CreateAssetMenu(fileName = "new SlotData", menuName = "SlotData")]
public class SlotData : ScriptableObject
{
    [Header("Slot Category")]
    [SerializeField]
    private ItemCategory _slotCategory;
    [Header("Slot_Prefab")]
    [SerializeField]
    private GameObject _slotPrefab;
    [Header("Slot Count")]
    [SerializeField]
    private int _slotCount = 5;

    public ItemCategory SlotCategory => _slotCategory;
    public int SlotCount => _slotCount;
    public GameObject SlotPrefab => _slotPrefab;
}
