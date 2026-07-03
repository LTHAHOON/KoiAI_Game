using R3;
using UnityEngine;

public class PairwiseTest : MonoBehaviour
{
    private ReactiveProperty<int> _hp = new(100);
    private void Start()
    {
        //Pairwise()란? 이전 값을 기억해서 활용하는 함수이다. 
        _hp
            .Pairwise()
            .Subscribe(x =>
            {
                Debug.Log($"Current: {x.Current} Prev: {x.Previous}");
                Debug.Log($"Damage: {x.Previous - x.Current }");
            });
        _hp.Value = 90;
        _hp.Value = 80;
    }
}
