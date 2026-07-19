using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public abstract class PopUpWindow: MonoBehaviour
    {
        /// <summary>
        /// 열기가 다 되었을 경우 호출
        /// </summary>
        public virtual void OnCompleteOpen() { }

        /// <summary>
        /// 닫기가 다 되었을 경우 호출
        /// </summary>
        public virtual void OnCompleteClose() { }

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

        public TModel GetVisualModel() => _visualModel;
        public TViewInfo GetViewInfo() => _viewInfo;

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
