using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyView
    {
        private VisualElement _root;
        private Button _playButton;

        public LobbyView(VisualElement root, LobbyViewInfo lobbyViewInfo)
        {
            _root = root;
            _playButton = _root.Q<Button>(lobbyViewInfo.PlayBtnName);
        }

        public Button PlayButton => _playButton;
    }
}
