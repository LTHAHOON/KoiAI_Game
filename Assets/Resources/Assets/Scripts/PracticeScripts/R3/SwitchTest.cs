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
            .ThrottleLast(TimeSpan.FromMilliseconds(500))
            .Select(text => Search(text))
            .Switch()
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
