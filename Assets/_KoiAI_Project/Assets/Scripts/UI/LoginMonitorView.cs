using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using KoiAI.Interact;
using KoiAI.UI;
using KoiAI.Utilities;
using KoiAI.Utilities.FadeInOutColor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KoiAI.UI
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
        private CancellationTokenSource _hiddenInputFieldToken;

        private MonitorScreenView _parentMonitorScreenView;

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            if(parentWindow is not MonitorScreenView monitorScreenView)
            {
                return;
            }
            _parentMonitorScreenView = monitorScreenView;
            _pinCodeBarInitialPosition = _pinCodeBar.anchoredPosition;
            _pinCodeBarFade = _pinCodeBar.GetComponent<FadeInOutImageColor>();
            _loginScreen.alpha = 0f;
            _loginScreen.interactable = false;
            _loginScreen.blocksRaycasts = false;
            _loginScreen.gameObject.SetActive(false);
        }

        public override void OnBeginOpen()
        {
            ResetPin();
            _hiddenInputFieldToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _hiddenInputField_Code.onValueChanged.AddListener(OnValueChangedPinCode);
            _hiddenInputField_Code.onDeselect.AddListener(OnDeselect);
            _pinCodeBarFade.PlayFadeInOut();
            KeepFocusHiddenInputFieldAsync(_hiddenInputFieldToken.Token).Forget();
        }

        public override void OnBeginClose()
        {
            StopInput();
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _loginScreen;
            settings = _fadeSettings;
            return true;
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
                    _parentMonitorScreenView.OnInteractToCheckPin?.Invoke(new MonitorInteractContext(pinCode));
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

    }
}
