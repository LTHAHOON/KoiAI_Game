using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class FancySlider : Slider
{
    [UxmlAttribute]
    public Sprite DraggerSprite { get; set; }

    private VisualElement _dragger;
    public FancySlider()
    {
        RegisterCallback<AttachToPanelEvent>(OnAttachPanel);
    }


    private void OnAttachPanel(AttachToPanelEvent evt)
    {
        _dragger = this.Q("unity-dragger");

       if (_dragger != null)
        {
            _dragger.style.backgroundImage = new StyleBackground(DraggerSprite);
        }
    }
}
