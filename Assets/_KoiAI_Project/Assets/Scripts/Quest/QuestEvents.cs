using System;

namespace KoiAI.Quest
{
    public static class QuestEvents
    {
        public static Action<QuestData> OnQuestAccpeted;
        public static Action<QuestData> OnQuestCleared;
        public static Action<long, QuestObjectiveController> OnQuestObjectiveCleared;
        public static Action OnQuestObjectiveFailed;

    }
}
