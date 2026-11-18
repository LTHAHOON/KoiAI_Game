using DG.Tweening;
using KoiAI.Utilities;
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
        private readonly Dictionary<CanvasGroup, Tween> _fadeTweens = new();

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

            if (popUpWindow is PopUpWindow_Canvas canvasWindow &&
                canvasWindow.TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings))
            {
                ChangePopUpStateWithFade(popUpStateToChange, canvasWindow, canvasGroup, settings);
                return;
            }

            IPopUpWindowContainer windowContainer =  popUpWindow.GetIPopUpWindowContainer();
            if (windowContainer.CurrentPopUpState == popUpStateToChange) 
            {
                return;
            }
            
            windowContainer.SetCurrentPopUpState(popUpStateToChange);
            if (popUpStateToChange == PopUpState.Open)
                popUpWindow.OnBeginOpen();
            else
                popUpWindow.OnBeginClose();
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
            PopUpWindow[] remaining = _popUpWindowStack.Where(window => window && window != popUpWindow).Reverse().ToArray();
            _popUpWindowStack = new Stack<PopUpWindow>(remaining);
            if (popUpWindow.GetIPopUpWindowContainer().CurrentPopUpState == PopUpState.Open)
            {
                _popUpWindowStack.Push(popUpWindow);
            }
        }

        public void ChangePopUpStateWithFade(PopUpState state, PopUpWindow_Canvas popUpWindow, CanvasGroup canvasGroup, CanvasGroupFadeSettings settings)
        {
            if (!popUpWindow || !canvasGroup)
            {
                return;
            }

            PopUpWindowContainer<Transform, GameObject> container = popUpWindow.GetPopUpWindowContainer();
            if (container.CurrentPopUpState == state)
            {
                return;
            }

            container.SetCurrentPopUpState(state);
            RefreshPopUpStack(popUpWindow);
            bool opening = state == PopUpState.Open;
            if (opening)
            {
                container.Window.SetActive(true);
                popUpWindow.OnBeginOpen();
            }
            else
            {
                popUpWindow.OnBeginClose();
            }

            ChangeCanvasGroupState(state, canvasGroup, settings, () =>
            {
                if (!popUpWindow || container.CurrentPopUpState != state)
                {
                    return;
                }
                if (!opening)
                {
                    container.Window.SetActive(false);
                }
                CallOnCompleted(popUpWindow);
            });
        }

        public void ChangeCanvasGroupState(PopUpState state, CanvasGroup canvasGroup, CanvasGroupFadeSettings settings, Action onCompleted = null)
        {
            if (!canvasGroup)
            {
                return;
            }

            if (_fadeTweens.Remove(canvasGroup, out Tween previousTween))
            {
                previousTween.Kill();
            }

            bool opening = state == PopUpState.Open;
            canvasGroup.interactable = opening;
            canvasGroup.blocksRaycasts = opening;
            if (opening)
            {
                canvasGroup.gameObject.SetActive(true);
            }

            _fadeTweens[canvasGroup] = canvasGroup.DOFade(opening ? 1f : 0f, settings.Duration)
                .SetEase(settings.Ease)
                .SetUpdate(settings.UseUnscaledTime)
                .SetLink(canvasGroup.gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(() =>
                {
                    _fadeTweens.Remove(canvasGroup);
                    if (!canvasGroup)
                    {
                        return;
                    }
                    if (!opening)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }
                    onCompleted?.Invoke();
                });
        }

        private void OnDestroy()
        {
            foreach (Tween tween in _fadeTweens.Values)
            {
                tween.Kill();
            }
            _fadeTweens.Clear();
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
