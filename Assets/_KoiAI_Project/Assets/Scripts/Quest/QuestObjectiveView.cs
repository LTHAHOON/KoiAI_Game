using System.Text;
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

        private StringBuilder _sb;
        
        public void InitView(QuestObjectiveData questObjectiveData)
        {
            _description.text = questObjectiveData.Description;

            if(_sb == null)
            {
                _sb = new();     
            }
            
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
            if(_sb == null)
            {
                _sb = new();     
            }
            
            SetRequirementCount(curCount, questObjectiveData.RequirementCount);
            SetTime(curTime, questObjectiveData.TimeLimit);
        }

        private void SetRequirementCount(int curCount, int requirementCount)
        {
            _sb.Clear();
             if(requirementCount > 0)
            {
                _sb.Append("(");
                _sb.Append(curCount);
                _sb.Append("/");
                _sb.Append(requirementCount);
                _sb.Append(")");
                _requireAmount.SetText(_sb);
            }
        }

        private void SetTime(float curTime, float timeLimit)
        {
            _sb.Clear();
            if(timeLimit > 0)
            {
                _sb.Append("남은 시간:");
                _sb.Append($"{curTime:F0}");
                _sb.Append("/");
                _sb.Append(timeLimit);
            }
            _questTimer.SetText(_sb);    
        }
    }
}
