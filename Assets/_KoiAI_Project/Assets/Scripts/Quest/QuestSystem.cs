using System;
using System.Collections.Generic;
using KoiAI.Pool;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.Quest
{
    public class QuestSystem : MonoBehaviour
    {
        [SerializeField]
        private QuestView _questView;
        [SerializeField]
        private List<QuestData> _questDataList;
        [SerializeField]
        private QuestObjectiveView _questObjectivePrefab;
        [SerializeField]
        private PoolSize _questControlPoolSize;

        public Action OnAcceptQuest;
        public Action OnClearQuest;
        private Pool<QuestObjectiveView> _questObjectivePool;
        private List<QuestObjectiveView> _questInProgressList = new();
        private long _curQuestID;
        private void Start()
        {
            ulong questControlPrefabID = _questObjectivePrefab.GetEntityULongID();
            PoolManager.Instance.AddPool(questControlPrefabID, _questObjectivePrefab, _questControlPoolSize, PoolName.Quest);
            PoolManager.Instance.TryGetPool(questControlPrefabID, out _questObjectivePool);
            if (_questDataList != null && _questDataList.Count > 0)
            {
                _curQuestID = _questDataList[0].QuestID;    
                AcceptQuest(_curQuestID);
            }
        }

        public void AcceptQuest(long questID)
        {
            QuestData questData = GetQuestData(questID);
            _questView.SetView(questData);
            foreach(QuestObjectiveData objectiveData in questData.QuestObjectiveData)
            {
                QuestObjectiveView objectiveView = _questObjectivePool.Pop();
                objectiveView.SetView(objectiveData);      
                _questInProgressList.Add(objectiveView);
            }
            OnAcceptQuest?.Invoke();
        }

        public void ClearQuest(long questID)
        {
            QuestData questData = GetQuestData(questID);
            foreach(QuestObjectiveView objectiveView in _questInProgressList)
            {
                _questObjectivePool.Return(objectiveView);
            }
            _questInProgressList.Clear();

            OnClearQuest?.Invoke();
        }

        private QuestData GetQuestData(long questID)
        {
            for(int i = 0; i < _questDataList.Count; ++i)
            {
                if(_questDataList[i].QuestID == questID)
                {
                    return _questDataList[i];
                }
            }
            return null;
        }
    }
}
