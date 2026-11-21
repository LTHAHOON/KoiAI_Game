using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.UI
{
    public class MonitorHackingRewardView : PopUpWindow_Canvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private CanvasGroupFadeSettings _canvasGroupFadeSettings;

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
