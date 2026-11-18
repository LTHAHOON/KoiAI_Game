using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoiAI.Interact;
using KoiAI.UI;
using KoiAI.Utilities;
using KoiAI.Utilities.FadeInOutColor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KoiAI
{
    public class LoginMonitorView : PopUpWindow_Canvas
    {
        [SerializeField] 
        private CanvasGroup _loginScreen;
        [SerializeField] 
        private PinCodeInputField _hiddenInputField_Code;
        [SerializeField] 
        private TMP_Text[] _pinCodes;
        [SerializeField] 
        private RectTransform _pinCodeBar;
        [SerializeField] 
        private float _pinCodeOffset = 150f;
        [SerializeField] 
        private CanvasGroupFadeSettings _fadeSettings = new(0.3f);

        private FadeInOutImageColor _pinCodeBarFade;
        private Vector2 _pinCodeBarInitialPosition;
        private Tweener _screenTweener;
        private CancellationTokenSource _hiddenInputFieldToken;

        public event Action<MonitorInteractContext> OnSubmitPin;

        internal void CheckOrInitialize()
        {
            if (_screenTweener != null)
            {
                return;
            }

            _pinCodeBarInitialPosition = _pinCodeBar.anchoredPosition;
            _pinCodeBarFade = _pinCodeBar.GetComponent<FadeInOutImageColor>();
            _screenTweener = CanvasGroupTweenUtility.CreateFade(_loginScreen, true, _fadeSettings);
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
            ResetPin();
            _loginScreen.interactable = true;
            _loginScreen.blocksRaycasts = true;
            _loginScreen.gameObject.SetActive(true);
            _screenTweener.PlayForward();
            _hiddenInputFieldToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _hiddenInputField_Code.onValueChanged.AddListener(OnValueChangedPinCode);
            _hiddenInputField_Code.onDeselect.AddListener(OnDeselect);
            _pinCodeBarFade.PlayFadeInOut();
            KeepFocusHiddenInputFieldAsync(_hiddenInputFieldToken.Token).Forget();
        }

        public void Hide()
        {
            StopInput();
            _loginScreen.interactable = false;
            _loginScreen.blocksRaycasts = false;
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

        private void OnDeselect(string value)
        {
            KeepFocusHiddenInputFieldAsync(_hiddenInputFieldToken.Token).Forget();
        }

        private async UniTaskVoid KeepFocusHiddenInputFieldAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                await UniTask.NextFrame(cancellationToken: token);
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
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnValueChangedPinCode(string input)
        {
            if (input.Length > _pinCodes.Length)
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
                _pinCodeBar.anchoredPosition = _pinCodeBarInitialPosition + Vector2.right * (input.Length * _pinCodeOffset);
            }

            if (input.Length == _pinCodes.Length)
            {
                _pinCodeBarFade.StopFadeInOut();
                if (int.TryParse(input, out int pinCode))
                {
                    OnSubmitPin?.Invoke(new MonitorInteractContext(pinCode));
                }
            }
            else if (!_pinCodeBarFade.IsPlaying)
            {
                _pinCodeBarFade.PlayFadeInOut();
            }
        }

        private void ResetPin()
        {
            _hiddenInputField_Code.SetTextWithoutNotify(string.Empty);
            foreach (TMP_Text pinCode in _pinCodes)
            {
                pinCode.text = string.Empty;
            }
            _pinCodeBar.anchoredPosition = _pinCodeBarInitialPosition;
            _pinCodeBarFade.StopFadeInOut();
        }

        private void StopInput()
        {
            _hiddenInputField_Code.onValueChanged.RemoveListener(OnValueChangedPinCode);
            _hiddenInputField_Code.onDeselect.RemoveListener(OnDeselect);
            _hiddenInputFieldToken?.Cancel();
            _hiddenInputFieldToken?.Dispose();
            _hiddenInputFieldToken = null;
            _hiddenInputField_Code.DeactivateInputField();
        }

        private void OnDisable()
        {
            StopInput();
        }

        private void OnDestroy()
        {
            _screenTweener?.Kill();
        }
    }
}
