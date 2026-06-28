using System.Text;
using TMPro;
using UnityEngine;

namespace KoiAI.UI.HUD
{
    public class WeaponInfoControl : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _curCountText;
        [SerializeField]
        private TextMeshProUGUI _remainingCountText;

        private void Awake()
        {
            _curCountText.text = "0";
            _remainingCountText.text = "0";
        }

        public void SetCurCountText(StringBuilder sb)
        {
            _curCountText.SetText(sb);
        }
        public void SetRemainingCountText(StringBuilder sb)
        {
            _remainingCountText.SetText(sb);
        }
    }
}
