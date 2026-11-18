using System;
using System.Collections.Generic;
using DG.Tweening;
using KoiAI.Interact;
using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public class MonitorScreenView : PopUpWindow_Canvas
    {
        [SerializeField]
        private List<CanvasGroup> _hidingCanvasGroups;
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private Button _monitorScreenCloseButton;
        [SerializeField]
        private LoginMonitorView _loginMonitorView;
        [SerializeField]
        private MonitorHackingView _monitorHackingView;
        [SerializeField]
        private CanvasGroupFadeSettings _showFadeSettings = new CanvasGroupFadeSettings(0.3f);
        [SerializeField]
        private CanvasGroupFadeSettings _hideFadeSettings = new CanvasGroupFadeSettings(0.3f);

        private Tweener _screenTweener;
        private readonly List<Tweener> _hidingCanvasGroupTweeners = new();

        public Action<MonitorInteractContext> OnInteractToCheckPin;

        private void Awake()
        {
            CheckOrInitialize();
        }

        private void CheckOrInitialize()
        {
            if (_screenTweener != null)
            {
                return;
            }

            _screenTweener = CanvasGroupTweenUtility.CreateFade(_monitorScreen, true, _showFadeSettings);
            _screenTweener.onRewind += FinishHide;
            foreach (CanvasGroup canvasGroup in _hidingCanvasGroups)
            {
                _hidingCanvasGroupTweeners.Add(CanvasGroupTweenUtility.CreateFade(canvasGroup, false, _hideFadeSettings));
            }

            _loginMonitorView.OnSubmitPin += SubmitPin;
            _loginMonitorView.CheckOrInitialize();
            _monitorHackingView.CheckOrInitialize();
        }

        private void SubmitPin(MonitorInteractContext context)
        {
            if (_monitorScreen.interactable)
            {
                OnInteractToCheckPin?.Invoke(context);
            }
        }

        public void Show()
        {
            CheckOrInitialize();
            PopUpManager.Instance.ChangePopUpState(PopUpState.Open, this);
        }

        public override void OnCompleteOpen()
        {
            CheckOrInitialize();

            _monitorScreen.interactable = true;
            _monitorScreen.blocksRaycasts = true;
            _monitorScreen.gameObject.SetActive(true);
            _screenTweener.PlayForward();
            foreach (Tweener tweener in _hidingCanvasGroupTweeners)
            {
                tweener.PlayForward();
            }

            ChangeView(_loginMonitorView);
        }

        public void Hide()
        {
            _monitorHackingView.Hide();
            _loginMonitorView.Hide();
            TryBeginFadeOut();
        }

        private void TryBeginFadeOut()
        {
            foreach (Tweener tweener in _hidingCanvasGroupTweeners)
            {
                tweener.PlayBackwards();
            }
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

        public void ShowMonitorHackingView()
        {
            if (!_monitorScreen.interactable || _monitorHackingView.gameObject.activeSelf)
            {
                return;
            }

            ChangeView(_monitorHackingView);
        }

        public void HideMonitorHackingView()
        {
            if (!_monitorScreen.interactable || !_monitorHackingView.gameObject.activeSelf)
            {
                return;
            }

            ChangeView(_loginMonitorView);
        }

        private void ChangeView(PopUpWindow_Canvas nextView)
        {
            if (nextView == _monitorHackingView)
            {
                _loginMonitorView.Hide();
                _monitorHackingView.Show();
            }
            else if (nextView == _loginMonitorView)
            {
                _monitorHackingView.Hide();
                _loginMonitorView.Show();
            }
        }

        public void BindCloseButton(UnityAction onClose)
        {
            _monitorScreenCloseButton.onClick.AddListener(onClose);
        }

        private void OnDestroy()
        {
            if (_loginMonitorView != null)
            {
                _loginMonitorView.OnSubmitPin -= SubmitPin;
            }

            if (_monitorScreenCloseButton != null)
            {
                _monitorScreenCloseButton.onClick.RemoveListener(Hide);
            }
            _screenTweener?.Kill();
            foreach (Tweener tweener in _hidingCanvasGroupTweeners)
            {
                tweener.Kill();
            }
        }
    }
}
