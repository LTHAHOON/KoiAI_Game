using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using KoiAI.Interact;
    using KoiAI.Utilities;
    using KoiAI.Utilities.FadeInOutColor;

    public class MonitorScreenView : MonoBehaviour
    {
        [SerializeField]
        private List<CanvasGroup> _hidingCanvasGroups;
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private Button _monitorScreenCloseButton;
        [SerializeField]
        private PinCodeInputField _hiddenInputField_Code;
        [SerializeField]
        private TMP_Text[] _pinCodes;
        [SerializeField]
        private RectTransform _pinCodeBar;
        [SerializeField]
        private RectTransform _monitorHackingScreen;
        [SerializeField]
        private float _monitorScreen_Duration = 0.3f;
        [SerializeField]
        private float _hidingCanvasGroup_Duration = 0.3f;

        private FadeInOutImageColor _pinCodeBarFade;
        private float _pinCodeOffset = 150f;
        private Vector2 _pinCodeBarInitialPosition;
        private Tweener _screenTweener;
        private List<Tweener> _hidingCanvasGroupTweeners;
        private CancellationTokenSource _hiddenInputFieldToken;
        public Action<MonitorInteractContext> OnInteractToCheckPin;

        private void Awake()
        {
            _pinCodeBarInitialPosition = _pinCodeBar.anchoredPosition;
            _monitorScreen.alpha = 0f;
            _monitorScreen.gameObject.SetActive(false);
            _monitorHackingScreen.gameObject.SetActive(false);
            _pinCodeBarFade = _pinCodeBar.GetComponent<FadeInOutImageColor>();

            _screenTweener = _monitorScreen.DOFade(1f, _monitorScreen_Duration)
                .SetEase(Ease.Linear)
                .OnPlay(() =>
                {
                    if (!_screenTweener.isBackwards)
                    {
                        _monitorScreen.gameObject.SetActive(true);
                    }
                })
                .OnRewind(() =>
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

        private void OnEnable()
        {
            _hiddenInputFieldToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _hiddenInputField_Code.onValueChanged.AddListener(OnValueChangedPinCode);
            _hiddenInputField_Code.onDeselect.AddListener(OnDeselect);
            KeepFocusHiddenInputFieldAsync(_hiddenInputFieldToken.Token).Forget();
        }

        private void OnDeselect(string value)
        {
            KeepFocusHiddenInputFieldAsync(_hiddenInputFieldToken.Token).Forget();
        }

        private async UniTaskVoid KeepFocusHiddenInputFieldAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await UniTask.NextFrame();
                token.ThrowIfCancellationRequested();
                if (!isActiveAndEnabled)
                {
                    return;
                }

                if (!_hiddenInputField_Code.isFocused)
                {
                    EventSystem.current.SetSelectedGameObject(_hiddenInputField_Code.gameObject);
                    _hiddenInputField_Code.ActivateInputField();
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnValueChangedPinCode(string input)
        {
            if ((_hiddenInputField_Code.characterLimit > 0 && input.Length > _hiddenInputField_Code.characterLimit)
                || input.Length > _pinCodes.Length)
            {
                return;
            }

            for (int i = 0; i < _pinCodes.Length; i++)
            {
                if (i < input.Length)
                {
                    _pinCodes[i].SetTextFormat("{0}", input[i]);
                }
                else
                {
                    _pinCodes[i].text = string.Empty;
                }
            }

            if (input.Length != _pinCodes.Length)
            {
                float pinCodeBarX = _pinCodeBarInitialPosition.x + input.Length * _pinCodeOffset;
                _pinCodeBar.anchoredPosition = new Vector2(pinCodeBarX, _pinCodeBarInitialPosition.y);
            }

            if (input.Length == _pinCodes.Length)
            {
                _pinCodeBarFade.StopFadeInOut();
                if (int.TryParse(input, out int pinCode))
                {
                    OnInteractToCheckPin?.Invoke(new MonitorInteractContext(pinCode));
                }
            }
            else if (!_pinCodeBarFade.IsPlaying)
            {
                _pinCodeBarFade.PlayFadeInOut();
            }
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

        private void OnDisable()
        {
            _monitorScreenCloseButton.onClick.RemoveAllListeners();
            _hiddenInputField_Code.onValueChanged.RemoveAllListeners();
            _hiddenInputField_Code.onDeselect.RemoveAllListeners();
            _hiddenInputFieldToken?.Cancel();
            _hiddenInputFieldToken?.Dispose();
            _hiddenInputFieldToken = null;

            _hiddenInputField_Code.text = string.Empty;
            for (int i = 0; i < _pinCodes.Length; i++)
            {
                _pinCodes[i].text = string.Empty;
            }
            _pinCodeBar.anchoredPosition = new Vector2(_pinCodeBarInitialPosition.x, _pinCodeBarInitialPosition.y);
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
        }


    }
}
