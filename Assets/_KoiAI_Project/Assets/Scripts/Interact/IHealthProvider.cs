using UnityEngine;

namespace KoiAI.Interact
{
    public interface IHealthProvider : IItemPickUpConditionProvider
    {
        public float CurrentHealthRatio { get; }
    }
}
