using KoiAI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LoadingScreenPresenter : VisualPresenter<LoadingScreenView, LoadingScreenViewInfo>
    {
        [SerializeField]
        private UIScaleTransitionData _gameIconScaleTransitionData;

        private UIScaleTransition _gameIconScaleTransition;
        protected override void Initalize(UIDocument uiDoucument, LoadingScreenView visualView, LoadingScreenViewInfo visualViewInfo)
        {
            visualView = new(uiDoucument.rootVisualElement, visualViewInfo);

            _gameIconScaleTransition = new(gameObject, visualView.GameIconImage,_gameIconScaleTransitionData);
            _gameIconScaleTransition.ActivateTransition();

        }
    }
}
