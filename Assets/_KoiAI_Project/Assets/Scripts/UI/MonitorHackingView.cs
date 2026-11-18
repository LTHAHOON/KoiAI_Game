using System;
using DG.Tweening;
using UnityEngine;

namespace KoiAI.UI
{
    using KoiAI.Utilities;

    public class MonitorHackingView : PopUpWindow_Canvas
    {
        [SerializeField]
        private CanvasGroup _monitorHackingScreen;

        [SerializeField]
        private CanvasGroupFadeSettings _fadeSettings = new(0.3f);

        private Tweener _screenTweener;

        internal void CheckOrInitialize()
        {
            if (_screenTweener != null)
            {
                return;
            }

            _screenTweener = CanvasGroupTweenUtility.CreateFade(_monitorHackingScreen, true, _fadeSettings);
            _screenTweener.onRewind += FinishHide;
        }

        public void Show()
        {
            CheckOrInitialize();
            PopUpManager.Instance.ChangePopUpState(PopUpState.Open, this);
        }

        public override void OnCompleteOpen()
        {
            CheckOrInitialize();
            _monitorHackingScreen.interactable = true;
            _monitorHackingScreen.blocksRaycasts = true;
            _monitorHackingScreen.gameObject.SetActive(true);
            _screenTweener.PlayForward();
        }

        public void Hide()
        {
            _monitorHackingScreen.interactable = false;
            _monitorHackingScreen.blocksRaycasts = false;
            _screenTweener.PlayBackwards();
        }

        private void FinishHide()
        {
            if (!_screenTweener.isBackwards)
            {
                return;
            }
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        private void OnDestroy()
        {
            _screenTweener?.Kill();
        }
    }
}
