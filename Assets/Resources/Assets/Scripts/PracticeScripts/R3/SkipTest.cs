using R3;
using UnityEngine;
using UnityEngine.Rendering;

public class SkipTest : MonoBehaviour
{
    private void Start()
    {
        Observable.Range(0, 10)
            .Skip(5)
            .Subscribe(x => Debug.Log(x));
        
        ReactiveProperty<int> hp = new(100);

        hp
            //Skip이란? 말 그대로 정해진 횟수(프레임)만큼 스킵하는 것을 말한다. 현재 값을 출력하고 싶지 않을 때 쓰이기도 한다.
            //Take는 Skip의 반대라고 보면 된다. Take(2)일 경우 2번만 호출되고 구독을 해제시켜버린다.
            //SkipWhile이란? 조건이 true일 경우에만 다음 호출이 시작된다.
            .Skip(1)
            .SkipWhile(x => x < 50)
            .Subscribe(x =>
            {
                Debug.Log(x);
            });
    }
}
