using System;
using System.Collections.Generic;
using UnityEngine;

namespace KoiAI.UI
{


    [Serializable]
    public class PopUpWindowPair<TButton, TWindow>
    {
        [SerializeField]
        private TButton _openButton;
        [SerializeField]
        private TWindow _window;

        public TButton OpenButton => _openButton;
        public TWindow Window => _window;
    }

    [Serializable]
    public class PopUpWindowPairList<TButton, TWindow> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private PopUpWindowPair<TButton, TWindow>[] _arrPopUpWindowPairs;

        private Dictionary<TButton, TWindow> _dicPopUpWindowPair;

        //암시적으로 초기화되게 설정(데이터를 불러올 때 초기화됩니다.)
        public void OnAfterDeserialize() 
        {
            _dicPopUpWindowPair = new Dictionary<TButton, TWindow>();
            for (int i = 0; i < _arrPopUpWindowPairs.Length; i++)
            {
                PopUpWindowPair<TButton, TWindow> popUpWindowPair = _arrPopUpWindowPairs[i];
                _dicPopUpWindowPair.TryAdd(popUpWindowPair.OpenButton, popUpWindowPair.Window);
            }
        }

        public void OnBeforeSerialize() { }
    }
}
