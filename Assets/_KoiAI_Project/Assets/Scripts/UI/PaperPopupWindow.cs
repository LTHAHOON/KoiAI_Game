using KoiAI.Input;
using KoiAI.Item;
using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public sealed class PaperPopupWindow : PopUpWindow_Canvas
    {
        private static PaperPopupWindow _instance;

        [SerializeField]
        private CanvasGroup _paperPopup;
        [SerializeField]
        private Image _paperImage;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private CanvasGroupFadeSettings _fadeSettings = new(0.3f);

        private Sprite _paperSprite;

        public static void Open(PaperData paperData)
        {
            if (paperData == null)
            {
                return;
            }

            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PaperPopupWindow>(FindObjectsInactive.Include);
            }

            if (_instance == null || PopUpManager.Instance == null)
            {
                Debug.LogWarning("PaperPopupWindow is not present in the active scene.");
                return;
            }

            _instance.SetPaperTexture(paperData.PaperTex);
            PopUpManager.Instance.ChangePopUpState(PopUpState.Open, _instance);
        }

        private void Awake()
        {
            _instance = this;
            _paperPopup ??= GetComponent<CanvasGroup>();
            _paperImage ??= transform.Find("Paper_Image")?.GetComponent<Image>();
            _closeButton ??= transform.Find("Paper_CloseButton")?.GetComponent<Button>();

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }
        }

        private void SetPaperTexture(Texture2D paperTexture)
        {
            if (_paperImage == null)
            {
                return;
            }

            if (_paperSprite != null)
            {
                Destroy(_paperSprite);
                _paperSprite = null;
            }

            if (paperTexture == null)
            {
                _paperImage.sprite = null;
                return;
            }

            _paperSprite = Sprite.Create(
                paperTexture,
                new Rect(0f, 0f, paperTexture.width, paperTexture.height),
                new Vector2(0.5f, 0.5f));
            _paperImage.sprite = _paperSprite;
            _paperImage.preserveAspect = true;
        }

        private void Close()
        {
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        public override void OnBeginOpen()
        {
            InputService.SetEnableActionMap(InputActionMapContext.Global);
        }

        public override void OnCompleteClose()
        {
            InputService.SetEnableActionMap(InputActionMapContext.Player);
        }

        public override bool TryGetFadeSettings(
            out CanvasGroup canvasGroup,
            out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _paperPopup;
            settings = _fadeSettings;
            return canvasGroup != null;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }

            if (_paperSprite != null)
            {
                Destroy(_paperSprite);
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
