using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public class MonitorScreenView : MonoBehaviour
    {
        [SerializeField]
        private List<CanvasGroup> _hidingCanvasGroups;
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private Button _monitorScreenCloseButton;
        [SerializeField]
        private RectTransform _monitorHackingScreen;
        [SerializeField]
        private float _monitorScreen_Duration = 0.3f;
        [SerializeField]
        private float _hidingCanvasGroup_Duration = 0.3f;

        private Tweener _screenTweener;
        private List<Tweener> _hidingCanvasGroupTweeners;

        private void Awake()
        {
            _monitorScreen.alpha = 0f;
            _monitorScreen.gameObject.SetActive(false);
            _monitorHackingScreen.gameObject.SetActive(false);

            _screenTweener = _monitorScreen.DOFade(1f, _monitorScreen_Duration)
                .SetEase(Ease.Linear)
                .OnPlay(() =>
                {
                    if (!_screenTweener.isBackwards)
                    {
                        _monitorScreen.gameObject.SetActive(true);
                    }
                })
                .OnComplete(() =>
                {
                    if (_screenTweener.isBackwards)
                    {
                        _monitorScreen.gameObject.SetActive(false);
                    }
                })
                .SetAutoKill(false)
                .Pause();

            _hidingCanvasGroupTweeners = _hidingCanvasGroups
                .Select(canvasGroup => (Tweener)canvasGroup.DOFade(0f, _hidingCanvasGroup_Duration)
                    .SetEase(Ease.Linear)
                    .OnPlay(() =>
                    {
                        if (_screenTweener.isBackwards)
                        {
                            canvasGroup.gameObject.SetActive(true);
                        }
                    })
                    .OnComplete(() =>
                    {
                        if (!_screenTweener.isBackwards)
                        {
                            canvasGroup.gameObject.SetActive(false);
                        }
                    })
                    .SetAutoKill(false)
                    .Pause())
                .ToList();
        }

        public void Show()
        {
            _screenTweener.PlayForward();
            foreach (var tweener in _hidingCanvasGroupTweeners)
            {
                tweener.PlayForward();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            _screenTweener.PlayBackwards();
            foreach (var tweener in _hidingCanvasGroupTweeners)
            {
                tweener.PlayBackwards();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void BindCloseButton(UnityAction onClose)
        {
            if (_monitorScreenCloseButton == null)
            {
                Debug.LogError("MonitorScreenCloseButton이 할당되지 않았습니다.");
                return;
            }

            _monitorScreenCloseButton.onClick.AddListener(onClose);
        }

        private void OnDestroy()
        {
            _screenTweener?.Kill();

            if (_hidingCanvasGroupTweeners == null)
            {
                return;
            }
            foreach (Tweener tweener in _hidingCanvasGroupTweeners)
            {
                tweener?.Kill();
            }

            _monitorScreenCloseButton.onClick.RemoveAllListeners();
        }


    }
}
