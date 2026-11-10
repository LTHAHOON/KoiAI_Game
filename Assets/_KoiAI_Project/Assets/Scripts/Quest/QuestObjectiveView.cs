using TMPro;
using UnityEngine;

namespace KoiAI.Quest
{
    public class QuestObjectiveView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _description;
        [SerializeField]
        private TMP_Text _requireAmount;
        [SerializeField]
        private TMP_Text _questTimer;
        [SerializeField]
        private TMP_Text _clearText;

        public void InitView(QuestObjectiveData questObjectiveData)
        {
            _description.text = questObjectiveData.Description;

            SetRequirementCount(curCount: 0, questObjectiveData.RequirementCount);
            SetTime(curTime: 0, questObjectiveData.TimeLimit);
        }

        public void ClearView()
        {
            _requireAmount.gameObject.SetActive(false);
            _questTimer.gameObject.SetActive(false);
            _clearText.gameObject.SetActive(true);
        }

        public void RefreshView(QuestObjectiveData questObjectiveData, int curCount, float curTime)
        {

            SetRequirementCount(curCount, questObjectiveData.RequirementCount);
            SetTime(curTime, questObjectiveData.TimeLimit);
        }

        private void SetRequirementCount(int curCount, int requirementCount)
        {
            if (requirementCount <= 0)
            {
                return;
            }
            _requireAmount.SetText("({0}/{1})", curCount, requirementCount);
        }

        private void SetTime(float curTime, float timeLimit)
        {
            if (timeLimit <= 0)
            {
                return;
            }
            _questTimer.SetText("남은 시간: {0:0}/{1:0}", (int)curTime, (int)timeLimit);
        }
    }
}
