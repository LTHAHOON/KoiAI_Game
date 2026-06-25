using R3;
using UnityEngine;

public abstract class UIFollowHandle : MonoBehaviour
{
    private readonly Subject<FollowableUI> _uiFollowConnector = new();

    public Subject<FollowableUI> GetUIFollowConnector() => _uiFollowConnector;
}

public abstract class FollowableUI : MonoBehaviour
{
    [SerializeField]
    private bool _isBlockFollowUI;
    public UIFollowToObject GetUIFollowToOject()
    {
        if(TryGetComponent(out UIFollowToObject uiFollowToObj))
        {
            return uiFollowToObj;
        }
        return null;
    }
    public bool IsBlockFollowUI => _isBlockFollowUI;
}

[RequireComponent(typeof(UIFollowHandle))]
public class UIFollowConnector : MonoBehaviour
{
    private UIFollowHandle _uiFollowHandle;

    private void Awake()
    {
        //해당 오브젝트에 있는 핸들 가져오기
        _uiFollowHandle = GetComponent<UIFollowHandle>();

        var connector = _uiFollowHandle.GetUIFollowConnector();
        connector.Subscribe(followableUI =>
        {
            if(followableUI.IsBlockFollowUI)
            {
                return;
            }
            UIFollowToObject uiFollowToObj = followableUI.GetUIFollowToOject();
            if(uiFollowToObj)
            {
                uiFollowToObj.SetTargetObject(_uiFollowHandle.gameObject);
            }
        });
    }
}
