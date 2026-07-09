using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyPresenter : VisualPresenter<LobbyView, LobbyViewInfo>
    {
        protected override void Initalize(UIDocument uiDoucument, LobbyView visualView, LobbyViewInfo visualViewInfo)
        {
            visualView = new LobbyView(uiDoucument.rootVisualElement, visualViewInfo);
            visualView.PlayButton.clicked += OnClickPlayButton;
        }

        private void OnClickPlayButton()
        {
            Debug.Log("On Play");
        }
    }
}
