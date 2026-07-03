using R3;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    private void Start()
    {
        _button.OnClickAsObservable()
            .Subscribe(OnNextLog, OnCompleteLog)
            .AddTo(this);
    }

    private void OnNextLog(Unit unit)
    {
        Debug.Log("버튼 OnNextLog");
    }

    private void OnCompleteLog(Result result)
    {
        Debug.Log("버튼 OnCompleteLog");
    }
}
