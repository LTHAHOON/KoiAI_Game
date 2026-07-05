using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomSlider : MonoBehaviour
{
    private VisualElement _root;

    private VisualElement _slider;

    private VisualElement _dragger;

    private VisualElement _bar;

    private VisualElement _newDragger;
    
    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _slider = _root.Q<Slider>("MySlider");
        _dragger = _root.Q<VisualElement>("unity-dragger");
        AddElements();
        
        _slider.RegisterCallback<ChangeEvent<float>>(SliderValueChanged);
        _slider.RegisterCallback<GeometryChangedEvent>(SliderInit);
    }

    void AddElements()
    {
        _bar = new VisualElement();
        _dragger.Add(_bar);
        _bar.name = "Bar";
        _bar.AddToClassList("bar");
        
        _newDragger = new VisualElement();
        _slider.Add(_newDragger);
        _newDragger.name = "NewDragger";
        _newDragger.AddToClassList("newdragger");
        _newDragger.pickingMode = PickingMode.Ignore;
    }

    void SliderValueChanged(ChangeEvent<float> evt)
    {
        float width =  _newDragger.layout.width - _dragger.layout.width;
        float height = _newDragger.layout.height - _dragger.layout.height;
        Vector2 dist = new(width / 2, height / 2);
        Vector2 pos = _dragger.parent.LocalToWorld(_dragger.localBound.position);
        _newDragger.style.translate = _newDragger.parent.WorldToLocal(pos - dist);
    }

    void SliderInit(GeometryChangedEvent evt)
    {
        float width =  _newDragger.layout.width - _dragger.layout.width;
        float height = _newDragger.layout.height - _dragger.layout.height;
        Vector2 dist = new(width / 2, height / 2);
        Vector2 pos = _dragger.parent.LocalToWorld(_dragger.localBound.position);
        _newDragger.style.translate = _newDragger.parent.WorldToLocal(pos - dist);
    }
}
