using KoiAI.Core;
using KoiAI.Quest;
using UnityEngine;

namespace KoiAI.Quest
{
    public enum QuestKillType
    {
        Enemy,
        Item
    }

    public struct QuestKillData
    {
        private QuestKillType _questKillType;
        private long _targetID;
        private int _killCount;
        public QuestKillType QuestKillType => _questKillType;
        public long TargetID => _targetID;
        public int KillCount => _killCount;
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

        public void HandleEvent(QuestKillData questKillData, int killCount)
        {
            if(!IsValidTargetID(questKillData.TargetID))
            {
                return;
            }
            
            switch(questKillData.QuestKillType)
            {
                
            }
        }
    }
}
