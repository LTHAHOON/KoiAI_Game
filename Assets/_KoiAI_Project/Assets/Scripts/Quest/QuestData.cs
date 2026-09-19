using System;
using System.Collections.Generic;
using UnityEngine;

namespace KoiAI.Quest
{
    [CreateAssetMenu(fileName = "new QuestData", menuName = "KoiAI/QuestData")]
    public class QuestData : ScriptableObject
    {
        [SerializeField]
        private long _questID;
        [SerializeField]
        private string _questTitle;

        [SerializeField]
        private List<QuestObjectiveData> _questObjectiveData;

        public long QuestID => _questID;
        public string QuestTitle => _questTitle;
        public List<QuestObjectiveData> QuestObjectiveData => _questObjectiveData;
    }

    [Serializable]
    public enum QuestObjectiveType
    {
        KILL,
        COLLECTION,
        MOVEMENT,
        INTERACTION,
    }

    [Serializable]
    public class QuestObjectiveData
    {
        [SerializeField]
        private QuestObjectiveType _objectiveType;
        [SerializeField]
        private string _description;
        [SerializeField]
        private long _targetID;
        [SerializeField]
        private int _requirementCount;
        [SerializeField]
        private float _timeLimit;

        public QuestObjectiveType ObjectiveType => _objectiveType;
        public float TimeLimit => _timeLimit;
        public long TargetID => _targetID;
        public string Description => _description;
        public int RequirementCount => _requirementCount;
    }
}
