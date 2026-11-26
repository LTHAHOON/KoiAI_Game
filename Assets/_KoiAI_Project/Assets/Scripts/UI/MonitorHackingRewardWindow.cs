using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using DG.Tweening;
    using KoiAI.Player;
    using KoiAI.Utilities;

    public class MonitorHackingRewardWindow : PopUpWindow_Canvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private CanvasGroup _howToUseRewardCanvasGroup;
        [SerializeField]
        private CanvasGroupFadeSettings _canvasGroupFadeSettings;
        [SerializeField]
        private Button _getRewardButton;
        [SerializeField]
        private Button _howToUseRewardButton;
        [SerializeField]
        private Button _howToUseRewardCloseButton;

        private ChipController _chipController;
        private MonitorScreenWindow _parentMonitorScreenWindow;

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            if (parentWindow is not MonitorScreenWindow monitorScreenView)
            {
                return;
            }
            _parentMonitorScreenWindow = monitorScreenView;
            _chipController = FindAnyObjectByType<ChipController>();
            _getRewardButton.onClick.AddListener(OnGetRewardClicked);
            _howToUseRewardButton.onClick.AddListener(OnHowToUseRewardClicked);
            _howToUseRewardCloseButton.onClick.AddListener(OnCloseHowToUseReaward);
            _howToUseRewardCanvasGroup.alpha = 0f;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    
        private void OnHowToUseRewardClicked()
        {
            _howToUseRewardCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);
            _howToUseRewardCanvasGroup.interactable = true;
            _howToUseRewardCanvasGroup.blocksRaycasts = true;
        }

        private void OnCloseHowToUseReaward()
        {
            _howToUseRewardCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear);
            _howToUseRewardCanvasGroup.interactable = false;
            _howToUseRewardCanvasGroup.blocksRaycasts = false;
        }

        private void OnGetRewardClicked()
        {
            _parentMonitorScreenWindow.SetVisibleUI(true);
            _chipController.UnlockChip();
            _getRewardButton.onClick.RemoveAllListeners();
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _canvasGroup;
            settings = _canvasGroupFadeSettings;
            return true;
        }

        public override void OnCompleteOpen()
        {
            _parentMonitorScreenWindow.SetVisibleUI(false);
        }
    }
}
