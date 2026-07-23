using KoiAI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public enum PopUpState
    {
        Open, //열기
        Close, //닫기
    }

    public enum PopUpRenderType
    {
        Canvas,
        UIDocument,
    }

    public class PopUpManager : MonoBehaviour
    {
        private Stack<PopUpWindow> _popUpWindowStack;

        public static PopUpManager Instance { get; private set; }

        private void Awake()
        {
            _popUpWindowStack = new();
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            ConnectPopUpStateInput();
        }

        private void ConnectPopUpStateInput()
        {
            InputService.PlayerIA.Global.Cancel.performed += _ => ChangeCurrentPopUpState(PopUpState.Close);
        }

        private void ChangeCurrentPopUpState(PopUpState popUpStateToChange)
        {
            if (_popUpWindowStack.Count <= 0)
            {
                return;
            }
            PopUpWindow popUpWindow = _popUpWindowStack.Peek();
            ChangePopUpState(popUpStateToChange, popUpWindow);
        }

        public void ChangePopUpState(PopUpState popUpStateToChange, PopUpWindow popUpWindow)
        {
            if(!popUpWindow)
            {
                return;
            }

            IPopUpWindowContainer windowContainer =  popUpWindow.GetIPopUpWindowContainer();
            if (windowContainer.CurrentPopUpState == popUpStateToChange) 
            {
                return;
            }
            
            windowContainer.SetCurrentPopUpState(popUpStateToChange);
            switch (windowContainer.PopUpRenderType)
            {
                case PopUpRenderType.Canvas:
                    ChangePopUpState_Canvas(popUpStateToChange, (PopUpWindow_Canvas)popUpWindow);
                    break;
                case PopUpRenderType.UIDocument:
                    ChangePopUpState_UID(popUpStateToChange, (PopUpWindow_UID)popUpWindow);
                    break;
            }
        }

        private void ChangePopUpState_UID(PopUpState popUpStateToChange, PopUpWindow_UID popUpWindow)
        {
            if(popUpWindow == null)
            {
                return;
            }

            PopUpWindowContainer<UIDocument, string> windowContainer =  popUpWindow.GetPopUpWindowContainer();
            VisualElement window = windowContainer.Parent.rootVisualElement.Q<VisualElement>(windowContainer.Window);
            if (window != null)
            {
                bool visible = popUpStateToChange == PopUpState.Open;
                window.visible = visible;
            }
            RefreshPopUpStack(popUpWindow);
            CallOnCompleted(popUpWindow);
        }

        private void ChangePopUpState_Canvas(PopUpState popUpStateToChange, PopUpWindow_Canvas popUpWindow)
        {
            if (popUpWindow == null)
            {
                return;
            }

            PopUpWindowContainer<Transform, GameObject> windowContainer = popUpWindow.GetPopUpWindowContainer();
            bool active = popUpStateToChange == PopUpState.Open;

            if (windowContainer.Parent == windowContainer.Window.transform.parent)
            {
                windowContainer.Window.SetActive(active);
            }
            else
            {
                GameObject newWindow = Instantiate(windowContainer.Window, windowContainer.Parent);
                newWindow.SetActive(active);
            }
            RefreshPopUpStack(popUpWindow);
            CallOnCompleted(popUpWindow);
        }

        private void CallOnCompleted(PopUpWindow popUpWindow)
        {
            IPopUpWindowContainer windowContainer = popUpWindow.GetIPopUpWindowContainer();
            switch (windowContainer.CurrentPopUpState)
            {
                case PopUpState.Open:
                    popUpWindow.OnCompleteOpen();
                    break;
                case PopUpState.Close:
                    popUpWindow.OnCompleteClose();
                    break;
            }
        }


        private void RefreshPopUpStack(PopUpWindow popUpWindow)
        {
            IPopUpWindowContainer windowContainer = popUpWindow.GetIPopUpWindowContainer();
            switch(windowContainer.CurrentPopUpState)
            {
                case PopUpState.Open:
                    _popUpWindowStack.Push(popUpWindow);
                    break;
                case PopUpState.Close:
                    _popUpWindowStack.Pop();
                    break;
            }
        }
        public T FindPopUpWindow<T>(PopUpWindow[] popUpWindows) where T : PopUpWindow
        {
            for (int i = 0; i < popUpWindows.Length; i++)
            {
                Type typeToCompare = popUpWindows[i].GetType();
                if (typeToCompare == typeof(T))
                {
                    return (T)popUpWindows[i];
                }
            }
            return null;
        }
    }
}
