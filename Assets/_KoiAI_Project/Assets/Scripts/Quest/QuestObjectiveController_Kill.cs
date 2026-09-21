using KoiAI.Core;

namespace KoiAI.Quest
{
    public enum QuestKillType
    {
        PlayerOREnemy,
        Item
    }

    public struct QuestKillData
    {
        private QuestKillType _questKillType;
        private EntityIdentity _instigatorIdentity;
        private EntityIdentity _targetIdentity;
        private int _killCount;
        public QuestKillData(QuestKillType questKillType, EntityIdentity instiagtorIdentity, EntityIdentity targetIdentity, int killCount)
        {
            _questKillType = questKillType;
            _instigatorIdentity = instiagtorIdentity;
            _targetIdentity = targetIdentity;
            _killCount = killCount;
        }
        public readonly QuestKillType QuestKillType => _questKillType;
        public readonly EntityIdentity InstigatorIdentity => _instigatorIdentity;
        public readonly EntityIdentity TargetIdentity => _targetIdentity;
        public readonly int KillCount => _killCount;
    }

    public class QuestObjectiveController_Kill : QuestObjectiveController
    {
        public QuestObjectiveController_Kill(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView) : base(questID, objectiveData, objectiveView){}

        public override void Acppet()
        {
            GameplayEvents.OnKilled += HandleEvent;
        }

        public override void Clear()
        {
        }

        public void HandleEvent(QuestKillData questKillData)
        {
            if(!IsValidTargetID(questKillData.TargetIdentity.EntityID))
            {
                return;
            }
            
            switch(questKillData.QuestKillType)
            {
                case QuestKillType.PlayerOREnemy:
                    break;
            }
        }
    }
}
