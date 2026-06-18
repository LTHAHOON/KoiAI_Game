using R3;
using UnityEngine;

public class HPTest : MonoBehaviour
{
    //값이 바뀔 때마다 자동 감지하는 객체
    private ReactiveProperty<int> _hp = new(100);
    private void Start()
    {
        //AddTo()란? 오브젝트가 삭제될 때 자동으로 Dispose()호출해서 구독해제할수 있도록
        //데이터를 미리 추가하는 함수이다.
        //(gameObject를 넣으면 ObserverDestroyTrigger 컴포넌트가 추가되고
        //this 즉, 컴포넌트를 넣으면 MonoBehaviour에 있는 토큰을 가지고 생명주기를 연결하게 된다. )
        _hp.Subscribe(value =>
        {
            Debug.Log($"현재 체력: {value}");
        }).AddTo(gameObject);

        _hp.Value -= 10;
        _hp.Value -= 20;
    }

    private void OnDestroy()
    {
        //AddTO를 안할 경우 직접 호출
        //_hp?.Dispose();
    }
}
