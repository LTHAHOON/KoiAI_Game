using UnityEngine;

namespace KoiAI.UI
{
    [CreateAssetMenu(fileName = "new LoadingScreenViewInfo", menuName = "KoiAI/UI/ViewInfo/LoadingScreenViewInfo")]
    public class LoadingScreenViewInfo : VisualViewInfo
    {
        [SerializeField]
        private string _loadingSliderName;
        [SerializeField]
        private string _titleGameIconName;

        public string LoadingSliderName => _loadingSliderName;
        public string TitleGameIconName => _titleGameIconName;
    }
}
