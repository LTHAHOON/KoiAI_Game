using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace KoiAI.UI
{
    public enum PopUpButtonType
    {
        Open, //열기
        Close, //닫기
    }

    public enum PopUpWindowType
    {
        Canvas,
        UIDocument,
    }

    public class PopUpManager : MonoBehaviour
    {
        private Stack<PopUpWindowPair<Object, Object>> _popUpWindowPair;

        public static PopUpManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void ChangePopUpState(PopUpButtonType popUpButtonType)
        {
            switch (popUpButtonType)
            {
                case PopUpButtonType.Open:
                    OpenWindow();
                    break;
                case PopUpButtonType.Close:
                    CloseWindow();
                    break;
            }
        }

        public void OpenWindow()
        {

        }

        public void CloseWindow()
        {

        }
    }
}
