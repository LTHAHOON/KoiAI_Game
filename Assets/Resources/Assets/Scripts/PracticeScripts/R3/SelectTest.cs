using R3;
using UnityEngine;
using UnityEngine.UI;

public class SelectTest : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    [SerializeField]
    private Button _aButton;
    private void Start()
    {
        _button.OnClickAsObservable()
            //SelectMany란? Observable이 실행될 때 다른 Observable(Timer)을 자동으로 구독해서
            //2페이즈 실행을 하게 한다.
            //Select().Merge()와 동일하다.(Merge() 함수를 보면 확장 타입이 이중 타입인걸 알수 있다.)
            .Select(_ => Observable.Timer(System.TimeSpan.FromSeconds(2)))
            //Merge가 아닌 Switch()를 넣으면 기본 Observable이 구독 헤제되고 Timer가 구독된다.
            .Merge()
            .Subscribe(_ =>
            {
                Debug.Log("Select");
            });

        //예) 이펙트 처리 끝나면 데미지
        //Attack()
        //.SelectMany(_ => PlayEffect())
        //.SelectMany(_ => DealDamage())
    }

}
