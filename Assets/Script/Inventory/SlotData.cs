using UnityEngine;

[CreateAssetMenu(fileName = "new SlotData", menuName = "SlotData")]
public class SlotData : ScriptableObject
{
    [Header("Slot Type")]
    [SerializeField]
    private SlotType _slotType;
    [Header("Slot_Prefab")]
    [SerializeField]
    private Slot _slotPrefab;
    [Header("Slot Count")]
    [SerializeField]
    private int _slotCount = 5;

    public SlotType SlotType => _slotType;
    public int SlotCount => _slotCount;
    public Slot SlotPrefab => _slotPrefab;
}
