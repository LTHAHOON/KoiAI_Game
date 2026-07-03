using R3;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SubjectTest : MonoBehaviour
{
    //직접 이벤트 발생하게 하는 객체
    private Subject<int> _damageEvent = new();
    private void Start()
    {
        _damageEvent.Subscribe(damage =>
        {
            Debug.Log($"데미지: {damage}");
        }).AddTo(this);

        _damageEvent.OnNext(10);
        _damageEvent.OnNext(20);
    }

}
