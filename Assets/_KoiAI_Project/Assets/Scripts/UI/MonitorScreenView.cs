using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using KoiAI.Interact;
    using KoiAI.Utilities;

    public class MonitorScreenView : PopUpWindow_Canvas
    {
        [SerializeField]
        private List<PopUpWindow_Canvas> _childPopUpWindows;
        [SerializeField]
        private List<CanvasGroup> _hidingCanvasGroups;
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private Button _monitorScreenCloseButton;
        [SerializeField]
        private CanvasGroupFadeSettings _showFadeSettings = new CanvasGroupFadeSettings(0.3f);
        [SerializeField]
        private CanvasGroupFadeSettings _hideFadeSettings = new CanvasGroupFadeSettings(0.3f);

        private bool _initialized;

        public Action<MonitorInteractContext> OnInteractToCheckPin;
        public Action<bool> OnGetResultPinLogin;

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

            OpenChildView<LoginMonitorView>();
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

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            CheckOrInitialize();
            canvasGroup = _monitorScreen;
            settings = _showFadeSettings;
            return true;
        }

        public void SetCloseButtonInteractable(bool interactable)
        {
            _monitorScreenCloseButton.interactable = interactable;    
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
