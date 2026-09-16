using System.Collections.Generic;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.SurroundPos
{
    public class SurroundPosManager : MonoBehaviour
    {
        [Tooltip("링안에 사이 간격")]
        [SerializeField]
        private float _surroundInterval;
        [Tooltip("링 최대 갯수")]
        [SerializeField]
        private int _ringMaxCount;
        [Tooltip("링 사이 간격")]
        [SerializeField]
        private float _ringInterval;
        [Tooltip("첫번째 링 지름")]
        [SerializeField]
        private float _centerRadius;

        private Dictionary<ulong, List<SurroundPosSlot>> _surroundPosSlots;

        public static SurroundPosManager Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
            _surroundPosSlots = new();
        }

        public bool TryGetSurroundPos(SurroundPosContext context, GameObject target, out SurroundPosSlot surroundPosSlot)
        {
            surroundPosSlot = default;
            if (context == null || !target)
            {
                return false;
            }

            ulong targetID = target.GetEntityULongID();
            if (!_surroundPosSlots.TryGetValue(targetID, out var surroundPosSlots))
            {
                surroundPosSlots = new();
                _surroundPosSlots.Add(targetID, surroundPosSlots);
            }

            //삭제된 Slot이 있을 경우 제거
            surroundPosSlots.RemoveAll(x => x.Owner == null);

            int slotIndex = surroundPosSlots.FindIndex(slot => slot.Owner == context.Owner);
            if (slotIndex < 0)
            {
                slotIndex = surroundPosSlots.Count;
            }

            Vector3 centerPos = target.transform.position;
            float requiredDistance = _centerRadius + context.SurroundRadius + _surroundInterval;
            int ringSlotIndex = slotIndex;

            for (int ringIndex = 0; ringIndex < _ringMaxCount; ringIndex++)
            {
                float ringRadius = GetRingRadius(ringIndex, requiredDistance);
                int ringSlotCount = GetSlotCountPerRing(ringRadius, requiredDistance);

                if (ringSlotIndex < ringSlotCount)
                {
                    float angle = 2f * Mathf.PI * ringSlotIndex / ringSlotCount;
                    Vector3 direction = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    
                    surroundPosSlot = new SurroundPosSlot
                    {
                        Owner = context.Owner,
                        Position = centerPos + direction * ringRadius
                    };
                    
                    if (slotIndex == surroundPosSlots.Count)
                    {
                        surroundPosSlots.Add(surroundPosSlot);
                    }
                    else
                    {
                        surroundPosSlots[slotIndex] = surroundPosSlot;
                    }

                    return true;
                }

                ringSlotIndex -= ringSlotCount;
            }

            return false;
        }


        private int GetSlotCountPerRing(float ringRadius, float requiredDistance)
        {
            return Mathf.Max(1, Mathf.FloorToInt(2f * Mathf.PI * ringRadius / requiredDistance));
        }

        private float GetRingRadius(int ringIndex, float requiredDistance)
        {
            return requiredDistance + ringIndex * _ringInterval;
        }

    }
}
