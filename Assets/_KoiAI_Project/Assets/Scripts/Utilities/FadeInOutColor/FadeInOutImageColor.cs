using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.Utilities.FadeInOutColor
{
    [RequireComponent(typeof(Image))]
    public class FadeInOutImageColor : FadeInOutColor<Image>
    {
        private Image _image;

        protected override void Init()
        {
            _image = GetUIComponent();
            _image.color = GetBaseColor();
        }

        protected override void SetCurrentColor(Color color)
        {
            if (_image)
            {
                _image.color = color;
            }

        }
        protected override Color GetCurrentColor()
        {
            if (_image)
            {
                return _image.color;
            }
            return GetBaseColor();
        }
    }
}
