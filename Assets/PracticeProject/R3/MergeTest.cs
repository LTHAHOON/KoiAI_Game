using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MergeTest : MonoBehaviour
{
    private ReactiveProperty<int> _hp = new(10);
    private Subject<bool> _instaTrap = new();
    private Subject<bool> _timeOut = new();
    private Subject<bool> _dieSubject = new();
    private void Start()
    {
        _hp
            .Where(hp => hp <= 0)
            .Subscribe(_ => _dieSubject.OnNext(true))
            .AddTo(this);

        _dieSubject
            .Merge(_timeOut)
            .Merge(_instaTrap)
            .Subscribe(_ => Debug.Log("죽었습니다."))
            .AddTo(this);

    }
    private void Update()
    {
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            _hp.Value--;
            Debug.Log(_hp.CurrentValue);
        }
    }
}
