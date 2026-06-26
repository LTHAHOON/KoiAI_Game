using UnityEngine;
using UnityEngine.UI;

public class HealthImageMaterialBar : HealthBar
{
    [SerializeField]
    private Image _healthImage;
    [SerializeField]
    private string _healthAmountProperty = "_Gauge_Amount";

    private int _healthAmountID = -1;
    private Material _material;
    public override void SetHealthBarValue(float normHealth)
    {
        if(!_material)
        {
            _material = _healthImage.material;
        }
        if(_healthAmountID < 0)
        {
            _healthAmountID = Shader.PropertyToID(_healthAmountProperty);
        }

        //같은 material이 있을 경우 주의 요망
        _material.SetFloat(_healthAmountID, normHealth);
    }
}
