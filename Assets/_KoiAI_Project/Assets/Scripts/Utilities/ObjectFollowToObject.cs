using UnityEngine;

namespace KoiAI.Utilities
{
    public class ObjectFollowToObject : MonoBehaviour
    {
        [SerializeField]
        private Transform _targetTransform;
        [SerializeField]
        private float _followSpeed;
        [SerializeField]
        private Vector3 _targetOffset = Vector3.zero;
        [SerializeField] 
        private float _adjustFollowY = -0.1f;
        
        private Vector3 _myPos;
        private float _curTime = 0f;
        
        private void Awake()
        {
            if (!_targetTransform)
            {
                return;
            }
            _myPos = _targetTransform.position;
        }

        private void FixedUpdate()
        {
            if (!_targetTransform)
            {
                return;
            }
            Vector3 targetPos = _targetTransform.position + _targetOffset;
        
            if ((targetPos -_myPos).sqrMagnitude < 0.001f)
            {
                transform.position = _myPos;
                return;
            }

            _myPos = Vector3.Lerp(_myPos,targetPos, _followSpeed * Time.fixedDeltaTime);
            if (_targetTransform.position.y > _myPos.y)
            {
                _myPos.y = Mathf.Lerp(_myPos.y, _targetTransform.position.y + _adjustFollowY, _followSpeed * Time.fixedDeltaTime);
            }
            transform.position = _myPos;
        }
    }
}
