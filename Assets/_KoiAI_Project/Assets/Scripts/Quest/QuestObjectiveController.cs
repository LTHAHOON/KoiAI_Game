

using System;

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
        
        public bool IsValidTargetID(Guid targetID)
        {
            bool isValid = targetID == _objectiveData.TargetID;
            return isValid;
        }

        public long QuestID => _questID;
        public QuestObjectiveView ObjectiveView => _objectiveView;
        public QuestObjectiveData ObjectiveData => _objectiveData;
        
    }
}
