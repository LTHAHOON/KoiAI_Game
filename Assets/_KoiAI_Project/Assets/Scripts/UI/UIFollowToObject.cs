using UnityEngine;

namespace KoiAI.Utilities
{
    using KoiAI.UI;
    
    [DefaultExecutionOrder(300)]
    [RequireComponent(typeof(RectTransform))]
    public class UIFollowToObject : MonoBehaviour
    {
        [SerializeField]
        CanvasType _parentCanvasType;
        [SerializeField]
        private GameObject _targetObject;
        [SerializeField]
        private Vector3 _viewPortOffset;
        [SerializeField]
        private Vector3 _worldOffset;
        [SerializeField]
        private Vector3 _scaleOffset;
        [SerializeField]
        private float _maxDistance;
        [SerializeField]
        private float _scaleMax;
        [SerializeField]
        private float _scaleMin;
    
        private RectTransform _rectTransform;
        private UnityEngine.Camera _camera;
        private void Awake()
        {
            _camera = UnityEngine.Camera.main;
            _rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            UIFollowProcess();
        }

        private void UIFollowProcess()
        {
            if (!_targetObject)
            {
                return;
            }
            Vector3 targetWorldPos = _targetObject.transform.position + _worldOffset;
            Vector3 targetViewPortPos = _camera.WorldToViewportPoint(targetWorldPos) + _viewPortOffset;
            if (targetViewPortPos.z <= 0)
                return;
            _rectTransform.anchoredPosition = UIManager.Instance.ViewPortToAnchoredPoint(targetViewPortPos, _parentCanvasType);

            Vector3 dir = (_camera.transform.position - _targetObject.transform.position);
            float distance = dir.sqrMagnitude;
            float scale = distance / _maxDistance;
            scale = Mathf.Clamp(scale, _scaleMin, _scaleMax);
            Vector3 scaleVector = new Vector3(scale, scale, scale);
            transform.localScale = scaleVector + _scaleOffset;
        }

        public void SetTargetObject(GameObject targetObj)
        {
            _targetObject = targetObj;
        }
    }
}
