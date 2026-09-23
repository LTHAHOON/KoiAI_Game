
namespace KoiAI.Quest
{
    using KoiAI.Core;

    public class QuestObjectiveController_Collection : QuestObjectiveController
    {
        public QuestObjectiveController_Collection(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView) : base(questID, objectiveData, objectiveView){ }

        public override void AcppetObjective()
        {
            GameplayEvents.OnCollected += HandleEvent;
        }

        public override void ClearObjective()
        {
            GameplayEvents.OnCollected -= HandleEvent;
            QuestEvents.OnQuestObjectiveCleared.Invoke(QuestID, this);
        }

        public override void FailObjective()
        {
            GameplayEvents.OnCollected -= HandleEvent;
            QuestEvents.OnQuestObjectiveFailed.Invoke();
        }
    }
}
