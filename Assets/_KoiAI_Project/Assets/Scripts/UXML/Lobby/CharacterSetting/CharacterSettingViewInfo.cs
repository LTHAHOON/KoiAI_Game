using UnityEngine;
using UnityEngine.UIElements;

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

        [SerializeField]
        private string _costumeSlotList;
        [SerializeField]
        private string _costumeScrollViewName;
        [SerializeField]
        private string _costumeSearchRadioGroupName;
        [SerializeField]
        private string _costumeTextName;
        [SerializeField]
        private string _costumeImageName;
        [SerializeField]
        private string _costumeWearBtnName;
        [SerializeField]
        private string _costumeWearingBtnName;
        [SerializeField]
        private VisualTreeAsset _costumeSlotTemplate;

        public string CharacterScrollerName => _characterScrollerName;
        public string CharacterScrollerLabelName => _characterScrollerLabelName;

        public string ConfirmBtnName => _confirmBtnName;
        public string CancelBtnName => _cancelBtnName;

        public string CircleColorPickerName_Face => _circleColorPickerName_Face;
        public string CircleColorPickerName_Body => _circleColorPickerName_Body;
        public string CirclePaletteName => _circlePaletteName;
        public string PalettePickerName => _palettePickerName;
        public string PalettePickerCenterBtnName => _palettePickerCenterBtnName;

        public string CostumeSlotList => _costumeSlotList;
        public string CostumeScrollView => _costumeScrollViewName;
        public string CostumeSearchRadioGroupName => _costumeSearchRadioGroupName;
        public string CostumeTextName => _costumeTextName;
        public string CostumeImageName => _costumeImageName;
        public string CostumeWearBtnName => _costumeWearBtnName;
        public string CostumeWearingBtnName => _costumeWearingBtnName;

        public VisualTreeAsset CostumeSlotTemplate => _costumeSlotTemplate;
    }
}
