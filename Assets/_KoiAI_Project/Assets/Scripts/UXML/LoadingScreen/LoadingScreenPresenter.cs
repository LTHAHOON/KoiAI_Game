using KoiAI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    using KoiAI.Utilities;
    public class LoadingScreenPresenter : VisualPresenter<LoadingScreenView, LoadingScreenViewInfo>
    {
        [SerializeField]
        private UIScaleTransitionData _gameIconScaleTransitionData;
        public SceneReference _sceneReference;
        
        private UIScaleTransition _gameIconScaleTransition;
        protected override void Initalize(UIDocument uiDoucument, LoadingScreenView visualView, LoadingScreenViewInfo visualViewInfo)
        {
            visualView = new(uiDoucument.rootVisualElement, visualViewInfo);
            _gameIconScaleTransition = new(gameObject, visualView.GameIconImage,_gameIconScaleTransitionData);
            _gameIconScaleTransition.ActivateTransition();
            
            
            //드래그를 막기 위해 BubbleUp(버블단계)가 아닌 TrickleDown(캡쳐단계)에서 이벤트가 호출되도록합니다.
            //드래그 이벤트는 BubbleUp단계에서 실행되기 때문에 이보다 더 빠른 단계인 TrickleDown단계에서 해야합니다.
            visualView.LoadingSlider.RegisterCallback<PointerDownEvent>(e =>
            {
                //해당 UI의 모든 이벤트처리를 막아줍니다.(StopPropagation같은 경우 )
                e.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);

        }
    }
    
}
