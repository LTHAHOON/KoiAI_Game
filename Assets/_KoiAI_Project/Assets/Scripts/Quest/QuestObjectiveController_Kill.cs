
namespace KoiAI.Quest
{
    using KoiAI.Core;

    public class QuestObjectiveController_Kill : QuestObjectiveController
    {
        public QuestObjectiveController_Kill(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView) : base(questID, objectiveData, objectiveView){}

        public override void AcppetObjective()
        {
            GameplayEvents.OnKilled += HandleEvent;
        }

        public override void ClearObjective()
        {
            GameplayEvents.OnKilled -= HandleEvent;
            QuestEvents.OnQuestObjectiveCleared.Invoke(QuestID, this);
        }

        public override void FailObjective()
        {
            GameplayEvents.OnKilled -= HandleEvent;
            QuestEvents.OnQuestObjectiveFailed.Invoke();
            
        }
    }
}
