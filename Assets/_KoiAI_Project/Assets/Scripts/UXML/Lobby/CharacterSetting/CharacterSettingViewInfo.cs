using UnityEngine;

namespace KoiAI.UI
{
    [CreateAssetMenu(fileName = "new CharacterSettingViewInfo", menuName = "KoiAI/UI/ViewInfo/CharacterSettingViewInfo")]
    public class CharacterSettingViewInfo : VisualViewInfo
    {
        [SerializeField]
        private string _confirmBtnName;
        [SerializeField]
        private string _cancelBtnName;
        [SerializeField]
        private string _characterScrollerName;
        [SerializeField]
        private string _characterScrollerLabelName;

        public string CharacterScrollerName => _characterScrollerName;
        public string CharacterScrollerLabelName => _characterScrollerLabelName;

        public string ConfirmBtnName => _confirmBtnName;
        public string CancelBtnName => _cancelBtnName;
    }
}
