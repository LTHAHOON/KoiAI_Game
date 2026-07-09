using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    public class LoadingScreenView : VisualView<LoadingScreenViewInfo>
    {
        private Image _gameIconImage;
        private Slider _loadingSlider;

        public LoadingScreenView(VisualElement root, LoadingScreenViewInfo info) : base(root, info) { }

        protected override void Initalize(VisualElement root, LoadingScreenViewInfo info)
        {
            _gameIconImage = root.Q<Image>(info.TitleGameIconName);
            _loadingSlider = root.Q<Slider>(info.LoadingSliderName);
        }

        public Slider LoadingSlider => _loadingSlider;
        public Image GameIconImage => _gameIconImage;
    }
}
