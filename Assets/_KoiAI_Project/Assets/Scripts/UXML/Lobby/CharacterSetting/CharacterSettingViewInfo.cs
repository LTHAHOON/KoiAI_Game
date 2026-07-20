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
        
        [SerializeField] 
        private string _circleColorPickerName_Face;
        [SerializeField] 
        private string _circleColorPickerName_Body;
        [SerializeField]
        private string _circlePaletteName;
        [SerializeField]
        private string _palettePickerName;
        [SerializeField]
        private string _palettePickerCenterBtnName;

        public string CircleColorPickerName_Face => _circleColorPickerName_Face;
        public string CircleColorPickerName_Body => _circleColorPickerName_Body;
        public string CharacterScrollerName => _characterScrollerName;
        public string CharacterScrollerLabelName => _characterScrollerLabelName;

        public string ConfirmBtnName => _confirmBtnName;
        public string CancelBtnName => _cancelBtnName;

        public string CirclePaletteName => _circlePaletteName;
        public string PalettePickerName => _palettePickerName;
        public string PalettePickerCenterBtnName => _palettePickerCenterBtnName;

    }
}
