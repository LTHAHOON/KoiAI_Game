using System.Text;
using TMPro;
using UnityEngine;

public class WeaponInfoControl : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _curCountText;
    [SerializeField]
    private TextMeshProUGUI _remainingCountText;

    public void SetCurCountText(StringBuilder sb)
    {
        _curCountText.SetText(sb);
    }
    public void SetRemainingCountText(StringBuilder sb)
    {
        _remainingCountText.SetText(sb);
    }
}
