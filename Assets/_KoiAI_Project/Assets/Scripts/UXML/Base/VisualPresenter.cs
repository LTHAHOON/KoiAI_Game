using KoiAI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public abstract class VisualPresenter<TView, TViewInfo> : MonoBehaviour  where TView : VisualView<TViewInfo> where TViewInfo : VisualViewInfo
    {
        [SerializeField]
        private UIDocument _uiDoucument;
        [SerializeField]
        private TViewInfo _visualViewInfo;

        private TView _visualView;

        private void Awake()
        {
            if(!_uiDoucument && !_visualViewInfo)
            {
                return;
            }
            Initalize(_uiDoucument, ref _visualView, _visualViewInfo);
        }

        protected abstract void Initalize(UIDocument uiDoucument, ref TView visualView, TViewInfo visualViewInfo);
        
        public TView GetVisualView() => _visualView;
        public TViewInfo GetVisualViewInfo() => _visualViewInfo;
    }
}
