using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyPresenter : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _uiDoucument;
        [SerializeField]
        private LobbyViewInfo _lobbyViewInfo;

        private LobbyView _lobbyView;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _lobbyView = new LobbyView(_uiDoucument.rootVisualElement, _lobbyViewInfo);
            _lobbyView.PlayButton.clicked += OnClickPlayButton;
        }

        private void OnClickPlayButton()
        {
            Debug.Log("On Play");
        }
    }
}
