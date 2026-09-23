using System;

namespace KoiAI.Quest
{
    using KoiAI.Core;

    public abstract class QuestObjectiveController
    {
        private long _questID;
        private QuestObjectiveView _objectiveView;
        private QuestObjectiveModel _objectiveModel;
        public QuestObjectiveController(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView)
        {
            _questID = questID;
            _objectiveView = objectiveView;
            _objectiveModel = new(objectiveData);
            objectiveView.InitView(objectiveData);
        }

        public virtual void Tick(float deltaTime)
        {
            _objectiveModel.AddDeltaTime(deltaTime);
            _objectiveView.RefreshView(_objectiveModel.Data, _objectiveModel.CurrentCount, _objectiveModel.CurrentTime);
            EvaluateObjectiveState();
        }
        public virtual void HandleEvent(ObjectiveProgressEventData eventData)
        {
            if(eventData.TargetIdentity == null || !IsValidTargetID(eventData.TargetIdentity.EntityID))
            {
                return;
            }
            
            ObjectiveModel.AddCount(eventData.CountDelta);
        }
        
        public abstract void AcppetObjective();

        public abstract void ClearObjective();
        public abstract void FailObjective();
       

        public bool IsValidTargetID(Guid targetID)
        {
            bool isValid = targetID == _objectiveModel.Data.TargetID;
            return isValid;
        }

        public void EvaluateObjectiveState()
        {
            if(_objectiveModel.IsCompleted())
            {
                ClearObjective();
                _objectiveView.ClearView();
            }
            else if(_objectiveModel.IsFailed())
            {
                FailObjective();
            }
        }

        public long QuestID => _questID;
        public QuestObjectiveView ObjectiveView => _objectiveView;
        public QuestObjectiveModel ObjectiveModel => _objectiveModel;
        
    }
}
