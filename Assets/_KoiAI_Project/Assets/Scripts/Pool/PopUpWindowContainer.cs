using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public interface IPopUpWindowContainer 
    {
        public void SetCurrentPopUpState(PopUpState popUpState);
        public PopUpState CurrentPopUpState { get; }
        public PopUpRenderType PopUpRenderType { get; }

    }

    [Serializable]
    public class PopUpWindowContainer<TParent, TWindow> : IPopUpWindowContainer
    {
        [SerializeField]
        private TParent _parent;
        [SerializeField]
        private TWindow _window;
        [SerializeField]
        private PopUpRenderType _renderType;

        private PopUpState _currentPopUpState = PopUpState.Close;

        public void SetCurrentPopUpState(PopUpState popUpState)
        {
            _currentPopUpState = popUpState;
        }

        public TParent Parent => _parent;
        public TWindow Window => _window;
        public PopUpState CurrentPopUpState => _currentPopUpState;
        public PopUpRenderType PopUpRenderType => _renderType;
    }

}
