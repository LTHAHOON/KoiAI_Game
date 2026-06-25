using UnityEngine;

[CreateAssetMenu(fileName = "new SlotData", menuName = "SlotData")]
public class SlotData : ScriptableObject
{
    [Header("Slot Type")]
    [SerializeField]
    private ItemSlotType itemSlotType;
    [Header("Slot_Prefab")]
    [SerializeField]
    private Slot _slotPrefab;
    [Header("Slot Count")]
    [SerializeField]
    private int _slotCount = 5;

    public ItemSlotType ItemSlotType => itemSlotType;
    public int SlotCount => _slotCount;
    public Slot SlotPrefab => _slotPrefab;
}
