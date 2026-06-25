using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHPTest : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _hpText;

    public ReactiveProperty<int> _hp = new(100);
    private void Start()
    {
        _hp.Subscribe(value =>
        {
            _hpText.text = value.ToString();
        }).AddTo(this);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _hp.Value -= 10;
        }
    }
}
