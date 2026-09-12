using System.Text;
using TMPro;
using UnityEngine;

namespace KoiAI.Quest
{
    public class QuestView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _questTitle;

        private StringBuilder _sb = new();
        public void SetView(QuestData questData)
        {
            _sb.Clear();
            _sb.Append(questData.QuestTitle);
            _sb.Append(" 퀘스트");
            _questTitle.SetText(_sb);
        }
    }
}
