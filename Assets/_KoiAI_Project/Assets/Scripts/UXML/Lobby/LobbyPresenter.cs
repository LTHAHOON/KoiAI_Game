using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyPresenter : VisualPresenter<LobbyView, LobbyViewInfo>
    {
        [SerializeField]
        private SceneReference _sceneReference;

        protected override void Initalize(UIDocument uiDoucument, LobbyView visualView, LobbyViewInfo visualViewInfo)
        {
            visualView = new LobbyView(uiDoucument.rootVisualElement, visualViewInfo);
            visualView.PlayButton.clicked += OnClickPlayButton;
        }

        private void OnClickPlayButton()
        {
            IAsyncSceneLoadHandler handler =  AsyncSceneLoader.CreateAsyncSceneLoadHandler();
            handler.SetLoadSceneMode(LoadSceneMode.Single).StartLoadAsync(this, _sceneReference);
        }
    }
}
