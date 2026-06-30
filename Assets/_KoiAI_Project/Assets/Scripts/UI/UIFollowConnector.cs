using R3;
using UnityEngine;

namespace KoiAI.Utilities
{
    public abstract class UIFollowHandle : MonoBehaviour
    {
        private readonly Subject<FollowableUI> _uiFollowConnector = new();

        public Subject<FollowableUI> GetUIFollowConnector() => _uiFollowConnector;

        /// <summary>
        /// 중재자로 구독시킨 Subject를 호출해줍니다.
        /// </summary>
        public void SetUITargetObject(FollowableUI followableUI)
        {
            if (followableUI == null)
            {
                return;
            }
            _uiFollowConnector.OnNext(followableUI);
        }
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

    /// <summary>
    /// UIFollowHandle과 FollowableUI를 이어주는 중재자역할을 합니다.
    /// </summary>
    [RequireComponent(typeof(UIFollowHandle))]
    public class UIFollowConnector : MonoBehaviour
    {
        private UIFollowHandle _uiFollowHandle;

        private void Awake()
        {
            //해당 오브젝트에 있는 핸들 가져오기
            _uiFollowHandle = GetComponent<UIFollowHandle>();

            //핸들의 Subject를 가져와서 구독해줍니다.
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
            }).AddTo(this);
        }
    }
}