using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public abstract class PopUpWindow: MonoBehaviour
    {
        public abstract IPopUpWindowContainer GetIPopUpWindowContainer();

    }

    public abstract class PopUpWindow_UID<TModel, TViewInfo> : PopUpWindow_UID
    {
        [SerializeField]
        private TModel _visualModel;
        [SerializeField]
        private TViewInfo _viewInfo;
        private void Awake()
        {
            Initalize(_visualModel, _viewInfo);
        }
        public abstract void Initalize(TModel visualModel, TViewInfo viewInfo);
    }

    public abstract class PopUpWindow_UID : PopUpWindow
    {
        [SerializeField]
        private PopUpWindowContainer<UIDocument, string> _popUpWindowContainer;

        public PopUpWindowContainer<UIDocument, string> GetPopUpWindowContainer()
        {
            return _popUpWindowContainer;
        }

        public override IPopUpWindowContainer GetIPopUpWindowContainer()
        {
            return _popUpWindowContainer;
        }

    }

    public abstract class PopUpWindow_Canvas : PopUpWindow
    {
        [SerializeField]
        private PopUpWindowContainer<Transform, GameObject> _popUpWindowContainer;

        public PopUpWindowContainer<Transform, GameObject> GetPopUpWindowContainer()
        {
            return _popUpWindowContainer;
        }

        public override IPopUpWindowContainer GetIPopUpWindowContainer()
        {
            return _popUpWindowContainer;
        }
    }

}
