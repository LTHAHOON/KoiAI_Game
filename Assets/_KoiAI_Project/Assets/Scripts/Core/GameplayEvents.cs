using System;

namespace KoiAI.Core
{
    public readonly struct ObjectiveProgressEventData
    {
        public EntityIdentity TargetIdentity { get; }
        public EntityIdentity InstigatorIdentity { get; }
        public int CountDelta { get; }

        public ObjectiveProgressEventData(EntityIdentity instigatorIdentity, EntityIdentity targetIdentity, int countDelta = 1)
        {
            InstigatorIdentity = instigatorIdentity;
            TargetIdentity = targetIdentity;
            CountDelta = countDelta;
        }
    }

    /// <summary>
    /// 게임플레이 이벤트(킬, 수집 등등)
    /// </summary>
    public static class GameplayEvents
    {
        public static Action<ObjectiveProgressEventData> OnKilled;
        public static Action<ObjectiveProgressEventData> OnAreaEntered;
        public static Action<ObjectiveProgressEventData> OnCollected;
        public static Action<ObjectiveProgressEventData> OnInteracted;
    }
}
