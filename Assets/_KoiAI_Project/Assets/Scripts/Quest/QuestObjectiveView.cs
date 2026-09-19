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
        
        private QuestObjectiveController _objectiveController;
        private StringBuilder _sb;
        
        public void SetView(QuestObjectiveData questObjectiveData)
        {
            _description.text = questObjectiveData.Description;

            if(_sb == null)
            {
                _sb = new();     
            }
            _sb.Clear();
             if(questObjectiveData.RequirementCount > 0)
            {
                _sb.Append("(");
                _sb.Append(0);
                _sb.Append("/");
                _sb.Append(questObjectiveData.RequirementCount);
                _sb.Append(")");
                _requireAmount.SetText(_sb);
            }


            _sb.Clear();
            if(questObjectiveData.TimeLimit > 0)
            {
                _sb.Append("남은 시간:");
                _sb.Append(0);
                _sb.Append("/");
                _sb.Append(questObjectiveData.TimeLimit);
            }
            _questTimer.SetText(_sb);    
        }
    }
}
