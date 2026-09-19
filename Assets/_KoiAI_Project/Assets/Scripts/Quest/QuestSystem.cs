using System;
using System.Collections.Generic;
using System.Linq;
using KoiAI.Pool;
using KoiAI.Utilities;
using Unity.Mathematics;
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

        private Pool<QuestObjectiveView> _questObjectivePool;
        private List<QuestObjectiveController> _questInProgressList = new();

        private void Awake()
        {
            ulong questControlPrefabID = _questObjectivePrefab.GetEntityULongID();
            PoolManager.Instance.AddPool(questControlPrefabID, _questObjectivePrefab, _questControlPoolSize, PoolName.Quest);
            PoolManager.Instance.TryGetPool(questControlPrefabID, out _questObjectivePool);
            
        }

        private void Start()
        {
            if (_questDataList != null && _questDataList.Count > 0)
            {
                //첫 퀘스트 받기
                AcceptQuest(_questDataList[0].QuestID);
            }
        }

        public void AcceptQuest(long questID)
        {
            QuestData questData = GetQuestData(questID);
            foreach(QuestObjectiveData objectiveData in questData.QuestObjectiveData)
            {
                QuestObjectiveView objectiveView = _questObjectivePool.Pop();
                QuestObjectiveController objectiveController = CreateQuestObjectiveControl(questID, objectiveData, objectiveView);
                objectiveController.Acppet();
                _questInProgressList.Add(objectiveController);
            }
            QuestEvents.OnQuestAccpeted?.Invoke(questData);
        }

        public void ClearQuestObjeictive(long questID, QuestObjectiveController objectiveController)
        {
            _questObjectivePool.Return(objectiveController.ObjectiveView);
            _questInProgressList.Remove(objectiveController);
            int questInProgressCount =_questInProgressList.Count(controller => controller.QuestID == questID);
            if(questInProgressCount <= 0)
            {
                QuestData questData = GetQuestData(questID);
                QuestEvents.OnQuestCleared?.Invoke(questData);
            }
        }

        private QuestObjectiveController CreateQuestObjectiveControl(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView)
        {
            QuestObjectiveController newObjectiveController = null;
            switch(objectiveData.ObjectiveType)
            {
                case QuestObjectiveType.KILL:
                    newObjectiveController =  new QuestObjectiveController_Kill(questID, objectiveData, objectiveView);
                    break;
            }
            return newObjectiveController;
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
