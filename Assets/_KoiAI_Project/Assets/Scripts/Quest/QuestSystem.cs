using System.Collections.Generic;
using System.Linq;
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

        private Pool<QuestObjectiveView> _questObjectivePool;
        private List<QuestObjectiveController> _inProgressObjectives = new();
        private List<QuestObjectiveController> _completedObjectives = new();

        private int _curQuestIndex;
        private void Awake()
        {
            ulong questControlPrefabID = _questObjectivePrefab.GetEntityULongID();
            PoolManager.Instance.AddPool(questControlPrefabID, _questObjectivePrefab, _questControlPoolSize, PoolName.Quest);
            PoolManager.Instance.TryGetPool(questControlPrefabID, out _questObjectivePool);
            QuestEvents.OnQuestObjectiveCleared += ClearQuestObjective;
            QuestEvents.OnQuestObjectiveFailed += FailOuestObjective;

        }

        private void Start()
        {
            if (_questDataList != null && _questDataList.Count > 0)
            {
                _curQuestIndex = 0;
                //첫 퀘스트 받기
                AcceptQuest(_questDataList[_curQuestIndex].QuestID);
            }
        }

        private void Update()
        {
            for(int i = 0; i < _inProgressObjectives.Count; ++i)
            {
                _inProgressObjectives[i].Tick(Time.deltaTime);
            }
        }

        public void AcceptQuest(long questID)
        {
            QuestData questData = GetQuestData(questID);
            foreach(QuestObjectiveData objectiveData in questData.QuestObjectiveData)
            {
                QuestObjectiveView objectiveView = _questObjectivePool.Pop();
                QuestObjectiveController objectiveController = CreateQuestObjectiveControl(questID, objectiveData, objectiveView);
                objectiveController.AcppetObjective();
                _inProgressObjectives.Add(objectiveController);
            }
        }

        public void ClearQuestObjective(long questID, QuestObjectiveController objectiveController)
        {
            _inProgressObjectives.Remove(objectiveController);
            _completedObjectives.Add(objectiveController);

            int questInProgressCount =_inProgressObjectives.Count(controller => controller.QuestID == questID);
            if(questInProgressCount > 0)
            {
                return;
            }

            QuestData questData = GetQuestData(questID);
            foreach (QuestObjectiveController controller in _completedObjectives)
            {
                _questObjectivePool.Return(controller.ObjectiveView);
            }
            _completedObjectives.RemoveAll(objective => objective.QuestID == questID);
            Debug.Log($"Clear: {questData.QuestTitle}");
            _questView.ClearView(_questDataList[_curQuestIndex]);
        }

        public void FailOuestObjective()
        {
            Debug.Log("Fail");
            //나중에 선택사항 추가해서 삭제 안되게 하기
            foreach (QuestObjectiveController controller in _inProgressObjectives)
            {
                _questObjectivePool.Return(controller.ObjectiveView);
            }
            _inProgressObjectives.Clear();
            _completedObjectives.Clear();
            _questView.FailView(_questDataList[_curQuestIndex]);
        }

        private QuestObjectiveController CreateQuestObjectiveControl(long questID, QuestObjectiveData objectiveData, QuestObjectiveView objectiveView)
        {
            return objectiveData.ObjectiveType switch
            {
                QuestObjectiveType.KILL => new QuestObjectiveController_Kill(questID, objectiveData, objectiveView),
                QuestObjectiveType.COLLECTION =>  new QuestObjectiveController_Collection(questID, objectiveData, objectiveView),
                _ => null
            };
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
