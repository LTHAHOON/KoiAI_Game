using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public class ChipView : MonoBehaviour
    {
        [SerializeField]
        private Image _chipCoolTimeImage;
        [SerializeField] 
        private CanvasGroup _canvasGroup;
        
        public void SetVisible(bool visible)
        {
            float endValue = visible ? 1f : 0f;
            _canvasGroup.DOFade(endValue, 0.3f).SetEase(Ease.Linear);
        }

        public void SetChipCooltimeFiilAmount(float fillAmount)
        {
            _chipCoolTimeImage.fillAmount = fillAmount;
        }
    }
}
