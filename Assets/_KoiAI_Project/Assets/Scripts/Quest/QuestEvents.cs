using System;
using UnityEngine;

namespace KoiAI.Quest
{
    public static class QuestEvents
    {
        public static Action<QuestData> OnQuestAccpeted;
        public static Action<QuestData> OnQuestCleared;
        public static Action<long, QuestObjectiveController> OnQuestObjectiveCleared;
        public static Action OnQuestObjectiveFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            OnQuestAccpeted = null;
            OnQuestCleared = null;
            OnQuestObjectiveCleared = null;
            OnQuestObjectiveFailed = null;
        }
    }
}
