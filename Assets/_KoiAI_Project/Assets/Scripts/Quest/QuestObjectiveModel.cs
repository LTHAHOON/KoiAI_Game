using System;
using UnityEngine;

namespace KoiAI.Quest
{
    [Serializable]
    public class QuestObjectiveModel
    {
        private readonly QuestObjectiveData _data;
        private int _currentCount;
        private float _currentTime;

        public QuestObjectiveModel(QuestObjectiveData data)
        {
            _data = data;
            _currentCount = 0;
            _currentTime = 0f;
        }

        public void AddCount(int amount)
        {
            _currentCount += amount;
        }

        public void AddDeltaTime(float deltaTime)
        {
            if(IsTimeOver)
            {
                return;
            }
            _currentTime += deltaTime;
        }
        
        public bool IsCompleted() => _currentCount >= _data.RequirementCount && !IsTimeOver;

        public bool IsFailed() => IsTimeOver;

        public QuestObjectiveData Data => _data;
        public int CurrentCount => _currentCount;
        public float CurrentTime => _currentTime;

        private bool IsTimeOver => _data.TimeLimit > 0f && _currentTime >= _data.TimeLimit;
    }
}
