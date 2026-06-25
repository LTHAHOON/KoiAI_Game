using System;
using R3;
using UnityEngine;

public class UpdateTest : MonoBehaviour
{
    public int HP = 100;
    private void Start()
    {
        //Observable.Return(10)
        //Observable.Timer(TimeSpan.FromSeconds(1))
        //Observable.FromAsync(...) 같은 경우 자동으로 구독해제되어서 AddTo없어도 안전
        
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
           //     Debug.Log("업데이트");
            })
            .AddTo(this);
        
        //EveryValueChanged란? 값이 바뀔 경우에만 실행된다.
        Observable.Return(HP)
            .Merge(Observable.EveryValueChanged(this, x => x.HP).Skip(1))
            .Subscribe(hp => Debug.Log($"HP: {hp}"))
            .AddTo(this);

        //EveryValueChanged는 사실상 유니티가 제공하는 타입을 쓸 경우에만 많이 쓰인다. 기본 타입들은 ReactiveProperty를 쓴다.
        /*
        Observable.EveryValueChanged(
                animator,
                x => x.GetCurrentAnimatorStateInfo(0).shortNameHash)
            .Subscribe(hash =>
            {
                Debug.Log("상태 변경");
            });
         */
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HP -= 10;
        }
    }
}
