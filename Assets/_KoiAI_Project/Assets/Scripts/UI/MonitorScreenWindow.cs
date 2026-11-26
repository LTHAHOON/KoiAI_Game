using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using DG.Tweening;
    using KoiAI.Input;
    using KoiAI.Interact;
    using KoiAI.Utilities;

    public class MonitorScreenWindow : PopUpWindow_Canvas
    {
        [SerializeField]
        private List<PopUpWindow_Canvas> _childPopUpWindows;
        [SerializeField]
        private List<CanvasGroup> _hidingCanvasGroups;
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private CanvasGroup _monitorScreen_UIGroup;
        [SerializeField]
        private Button _monitorScreenCloseButton;
        [SerializeField]
        private CanvasGroupFadeSettings _showFadeSettings = new CanvasGroupFadeSettings(0.3f);
        [SerializeField]
        private CanvasGroupFadeSettings _hideFadeSettings = new CanvasGroupFadeSettings(0.3f);

        private bool _initialized;

        public Action<MonitorInteractContext> OnInteractToCheckPin;
        public Action<bool> OnGetResultPinLogin;
        public event Action OnCloseCompleted;

        private void Awake()
        {
            CheckOrInitialize();
        }

        private void CheckOrInitialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _monitorScreen.alpha = 0f;
            _monitorScreen.interactable = false;
            _monitorScreen.blocksRaycasts = false;
            _monitorScreen.gameObject.SetActive(false);

            foreach (PopUpWindow_Canvas childPopUpWindow in _childPopUpWindows)
            {
                childPopUpWindow.InitializePopUp(this);
            }
        }

        public void Show()
        {
            CheckOrInitialize();
            PopUpManager.Instance.ChangePopUpState(PopUpState.Open, this);
        }

        public void Hide()
        {
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        public override void OnBeginOpen()
        {
            CheckOrInitialize();

            foreach (CanvasGroup canvasGroup in _hidingCanvasGroups)
            {
                PopUpManager.Instance.ChangeCanvasGroupState(PopUpState.Close, canvasGroup, _hideFadeSettings);
            }

            OpenChildView<LoginMonitorWindow>();
        }

        public override void OnBeginClose()
        {
            foreach (PopUpWindow_Canvas childPopUpWindow in _childPopUpWindows)
            {
                PopUpManager.Instance.ChangePopUpState(PopUpState.Close, childPopUpWindow);
            }
            foreach (CanvasGroup canvasGroup in _hidingCanvasGroups)
            {
                PopUpManager.Instance.ChangeCanvasGroupState(PopUpState.Open, canvasGroup, _hideFadeSettings);
            }
        }

        public override void OnCompleteClose()
        {
            OnCloseCompleted?.Invoke();
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            CheckOrInitialize();
            canvasGroup = _monitorScreen;
            settings = _showFadeSettings;
            return true;
        }

        public void SetVisibleUI(bool isVisible)
        {
            _monitorScreen_UIGroup.interactable = isVisible; 
            if(isVisible)
            {
                _monitorScreen_UIGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);
                InputService.PlayerIA.Monitor.Monitor_Cancel.Enable();
            }
            else
            {
                _monitorScreen_UIGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear);
                InputService.PlayerIA.Monitor.Monitor_Cancel.Disable();
            }
        }

        public void OpenChildView<T>() where T : PopUpWindow_Canvas
        {
            foreach (PopUpWindow_Canvas childPopUpWindow in _childPopUpWindows)
            {
                if(childPopUpWindow is T)
                {
                    PopUpManager.Instance.ChangePopUpState(PopUpState.Open, childPopUpWindow);
                    return;
                }
                PopUpManager.Instance.ChangePopUpState(PopUpState.Close, childPopUpWindow);
            }
        }

        public void BindCloseButton(UnityAction onClose)
        {
            _monitorScreenCloseButton.onClick.RemoveListener(onClose);
            _monitorScreenCloseButton.onClick.AddListener(onClose);
        }

        private void OnDestroy()
        {
            if (_monitorScreenCloseButton != null)
            {
                _monitorScreenCloseButton.onClick.RemoveListener(Hide);
            }

        }
    }
}
