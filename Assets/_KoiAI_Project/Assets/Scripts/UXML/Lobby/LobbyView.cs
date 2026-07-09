using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyView : VisualView<LobbyViewInfo>
    {
        private Button _playButton;

        public LobbyView(VisualElement root, LobbyViewInfo info) : base(root, info) { }

        protected override void Initalize(VisualElement root, LobbyViewInfo info)
        {
            _playButton = root.Q<Button>(info.PlayBtnName);
        }

        public Button PlayButton => _playButton;
    }
}
