using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using KoiAI.Input;
    using KoiAI.Item;
    using KoiAI.Utilities;

    public class PaperPopupWindow : PopUpWindow_Canvas
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
            
            _instance.SetPaperSprite(paperData.PaperSprite);
            PopUpManager.Instance.ChangePopUpState(PopUpState.Open, _instance);
        }

        private void Awake()
        {
            _instance = this;
            _closeButton.onClick.AddListener(Close);
            _paperPopup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void SetPaperSprite(Sprite paperSprite)
        {
            if (paperSprite == null)
            {
                _paperImage.sprite = null;
                return;
            }

            _paperImage.sprite = paperSprite;
            _paperImage.preserveAspect = true;
        }

        private void Close()
        {
            PopUpManager.Instance.ChangePopUpState(PopUpState.Close, this);
        }

        public override void OnBeginOpen()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            InputService.SetEnableActionMap(InputActionMapContext.Global);
        }

        public override void OnCompleteClose()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputService.SetEnableActionMap(InputActionMapContext.Player);
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _paperPopup;
            settings = _fadeSettings;
            return canvasGroup != null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            if(_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Close);
            }
        }
    }
}
