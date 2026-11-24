using KoiAI.Player;
using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public class MonitorHackingRewardView : PopUpWindow_Canvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private CanvasGroupFadeSettings _canvasGroupFadeSettings;
        [SerializeField]
        private Button _getRewardButton;

        private ChipController _chipController;
        private MonitorScreenView _parentMonitorScreenView;

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            if (parentWindow is not MonitorScreenView monitorScreenView)
            {
                return;
            }
            _parentMonitorScreenView = monitorScreenView;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _chipController = FindAnyObjectByType<ChipController>();
            _getRewardButton.onClick.AddListener(OnGetRewardClicked);
        }

        private void OnGetRewardClicked()
        {
            _parentMonitorScreenView.SetCloseButtonInteractable(true);
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
            _parentMonitorScreenView.SetCloseButtonInteractable(false);
        }
    }
}
