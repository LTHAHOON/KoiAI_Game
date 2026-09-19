

namespace KoiAI.Quest
{
    public abstract class QuestObjectiveController
    {
        private long _questID;
        private QuestObjectiveData _objectiveData;
        private QuestObjectiveView _objectiveView;
        public QuestObjectiveController(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView)
        {
            _questID = questID;
            _objectiveData = objectiveData;
            _objectiveView = objectiveView;
            objectiveView.SetView(objectiveData);
        }

        public abstract void Acppet();

        public abstract void Clear();
        
        public bool IsValidTargetID(long targetID)
        {
            bool isValid = targetID == _questID;
            return isValid;
        }

        public long QuestID => _questID;
        public QuestObjectiveView ObjectiveView => _objectiveView;
        public QuestObjectiveData ObjectiveData => _objectiveData;
        
    }
}
