using R3;
using System;
using System.Threading;
using UnityEngine;

public class SwitchTest : MonoBehaviour
{
    [SerializeField]
    private string _searchText = "";

    private ReactiveProperty<string> _searchReactive = new();

    //종료 시점 직접 제어할때 쓰임
    private CancellationTokenSource _cts = new();
    
    private Subject<string> _inputText = new();
    private void Start()
    {
        _searchReactive
            .Subscribe(text => _inputText.OnNext(text));

        _inputText
            //ThrottleLast란? 추가 샘플링이라고 하기도 하고 해당 시간이 지나면 다음 호출이 되는 역할이다. 
            //Debounce(Throttle)란? 해당 시간동안 입력이 멈출 때까지 다음 호출이 안되게 하는 역할을 한다.
            .Debounce(TimeSpan.FromMilliseconds(500))
            //DistinctUntilChanged란? 값이 변경 될때까지 중복을 제거하는 함수을 말한다.
            .DistinctUntilChanged()
           // .Select(text => text)
           // .Switch()
            .Subscribe(x => Debug.Log(x)).RegisterTo(_cts.Token);
        
        //RegisterTo(this.destroyCancellationToken)는 AddTo(this)와 동일
    }

    private void Update()
    {
        _searchReactive.Value = _searchText;
    }

    private Observable<string> Search(string keyward)
    {
        return Observable.Timer(TimeSpan.FromSeconds(2f))
            .Select(_ => $"검색 결과 {keyward}");
    }
}
