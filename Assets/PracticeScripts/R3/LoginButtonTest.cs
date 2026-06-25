using R3;
using UnityEngine;
using UnityEngine.UI;

public class LoginButtonTest : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    [SerializeField]
    private string ID = string.Empty;
    [SerializeField]
    private string Password = string.Empty;

    private ReactiveProperty<string> _id = new("");
    private ReactiveProperty<string> _password = new("");
    private void Start()
    {
        _id.CombineLatest(_password, (idText, passwordText) =>
        {
            return !string.IsNullOrEmpty(idText) && !string.IsNullOrEmpty(passwordText);
        })
        .Subscribe(canLogin => _button.interactable = canLogin)
        .AddTo(this);
    }

    private void LateUpdate()
    {
        _id.Value = ID;
        _password.Value = Password;
    }
}
