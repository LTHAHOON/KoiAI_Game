using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    [CreateAssetMenu(fileName = "new LobbyViewInfo", menuName = "KoiAI/UI/ViewInfo/LobbyViewInfo")]
    public class LobbyViewInfo : ScriptableObject
    {
        [SerializeField]
        private string _playBtnName;
        [SerializeField]
        private string _settingOpenBtnName;
        [SerializeField]
        private string _profileOpenBtnName;
        [SerializeField]
        private string _profileNameTextName;
        [SerializeField]
        private string _charSetttingOpenBtn;
        [SerializeField]
        private string _exitGameBtn;

        public string PlayBtnName => _playBtnName;
        public string SettingOpenBtnName => _settingOpenBtnName;
        public string ProfileOpenBtnName => _profileOpenBtnName;
        public string ProfileNameTextName => _profileNameTextName;
        public string CharSettingOpenBtn => _charSetttingOpenBtn;
        public string ExitGameBtn => _exitGameBtn;

    }
}
