using R3;
using UnityEngine;
using UnityEngine.UI;

public class BufferTest : MonoBehaviour
{
    //ReplaySubject란? bufferSize만큼 버퍼를 저장하여 이전 정보를 출력한다. 
    private ReplaySubject<int> _replay = new(1);
    private void Start()
    {
         
        _replay.OnNext(1);
        _replay.OnNext(2);
        
        _replay.Subscribe(value => Debug.Log(value));
        _replay.OnNext(3);
        //Replay()란? ReplaySubject와 특징이 똑같고 애는 구독을 해두고 Connect 를 호출해야 구독된 전체 호출이 된다
        var shared = Observable.Range(1, 5).Replay();
        shared.Subscribe(value => Debug.Log($"A: {value}"));
        shared.Subscribe(value => Debug.Log($"B: {value}"));
        shared.Connect();
    }
}
