using R3;
using UnityEngine;

public class TimerTest : MonoBehaviour
{
    private int _second;
    private void Start()
    {
        //시간주기는 Unity 기능의 Time.deltaTime이 사용되고 TimeSpan은 그저 설정값일 뿐입니다.
        Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                ++_second;
                Debug.Log($"Tick: {_second}");
            })
            .AddTo(this);
            
    }

}
