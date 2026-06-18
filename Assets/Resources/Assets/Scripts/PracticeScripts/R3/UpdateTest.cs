using R3;
using UnityEngine;

public class UpdateTest : MonoBehaviour
{
    private void Start()
    {
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                Debug.Log("업데이트");
            })
            .AddTo(this);
    }

}
