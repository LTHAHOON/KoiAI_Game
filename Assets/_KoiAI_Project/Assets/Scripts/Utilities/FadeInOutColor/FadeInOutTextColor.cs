using TMPro;
using UnityEngine;

namespace KoiAI.Utilities.FadeInOutColor
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class FadeInOutTextColor : FadeInOutColor<TextMeshProUGUI>
    {
        private TextMeshProUGUI _text;

        protected override void Init()
        {
            _text = GetUIComponent();
            _text.color = GetBaseColor();
        }

        protected override void SetCurrentColor(Color color)
        {
            if (_text)
            {
                _text.color = color;
            }
        }

        protected override Color GetCurrentColor()
        {
            return _text ? _text.color : GetBaseColor();
        }
    }
}
