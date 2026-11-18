using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.UI
{
    public class MonitorHackingView : PopUpWindow_Canvas
    {
        [SerializeField]
        private CanvasGroup _monitorHackingScreen;
        [SerializeField]
        private CanvasGroupFadeSettings _fadeSettings = new(0.3f);

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            _monitorHackingScreen.alpha = 0f;
            _monitorHackingScreen.interactable = false;
            _monitorHackingScreen.blocksRaycasts = false;
            _monitorHackingScreen.gameObject.SetActive(false);
        }
        
        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _monitorHackingScreen;
            settings = _fadeSettings;
            return true;
        }
    }
}
