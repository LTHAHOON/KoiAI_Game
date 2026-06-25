using R3;
using UnityEngine;

public class TakeUntilTest : MonoBehaviour
{
    private int _count = 0;
    private void Start()
    {
        /*
        Observable.EveryUpdate()
            //TakeUntil이란? ~할때까지 호출됨 
            .TakeUntil(Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Space)))
            .Subscribe(_ => Debug.Log("Update"), _ => Debug.Log("Complete"));
            */

        Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Select(_ => ++_count)
            .TakeUntil(Observable.Interval(System.TimeSpan.FromSeconds(5)))
            .Subscribe(x => Debug.Log(x));
        
        //Timer와 Interval차이는 Timer는 시간 끝난 즉시 구독 해제되고 Interval은 계속 실행된다.
    }

}
