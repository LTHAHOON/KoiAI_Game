using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class ObserveOnMainThreadTest : MonoBehaviour
{
    private void OnEnable()
    {
        //UnitTask.RunOnThreadPool(), Task.Run()은 작업쓰레드에서 작동. 즉, 멀티쓰레드
        //ObserveOnMainThread란? 강제로 작업 쓰레드를 메인 쓰레드로 옮기는 작업을 말한다. 
        //UniTask.Delay()는 비동기 작업이지만 메인쓰레드에서 작동함.
        Observable.FromAsync(async ct =>
            {
                return await UniTask.RunOnThreadPool(() =>
                {
                    //...이 순간에는 작업 쓰레드에서 작동하기 때문에 메인쓰레드는 다른 곳에 진행되는 중
                    return "Hello";
                });
            }).ObserveOnMainThread()
            .Subscribe(value => Debug.Log(value));
    }

    private void Start()
    {
        //비동기 작업중 먼저 실행이 되는 것을 볼 수 있음
        Debug.Log("Start");
    }
}
