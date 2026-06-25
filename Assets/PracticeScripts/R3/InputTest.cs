using R3;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    private void Start()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Space))
            .Subscribe(_ =>
            {
                Debug.Log("스페이스");
            })
            .AddTo(this);
    }
}
