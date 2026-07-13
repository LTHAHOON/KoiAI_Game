using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LobbyView : VisualView<LobbyViewInfo>
    {
        private Button _playButton;
        private Button _characterSettingButton;

        public LobbyView(VisualElement root, LobbyViewInfo info) : base(root, info) { }

        protected override void Initalize(VisualElement root, LobbyViewInfo info)
        {
            _playButton = root.Q<Button>(info.PlayBtnName);
            _characterSettingButton = root.Q<Button>(info.CharSettingOpenBtn);
        }

        public Button PlayButton => _playButton;
        public Button CharacterSettingButton => _characterSettingButton;
    }
}
