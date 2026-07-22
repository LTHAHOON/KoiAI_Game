using NaughtyAttributes;
using UnityEditor.Timeline;
using UnityEngine;

//Never used Warning 차단
#pragma warning disable CS0414

namespace KoiAI.CustomPhysics
{
    public class GravityControl : MonoBehaviour
    {
        [DisableIf(nameof(_initCompleted))]
        [SerializeField]
        private GravityData _gravityData;

        private Vector3 _checkCollisionPos;
        private Collider _collider;
        private bool _initCompleted = false;
        private readonly float _checkCollisionDist = 0.5f;
        private float _groundCheckDist;
        private float _curTime;
        private float _startTime;
        private float _curSpeed = 0f;
        private float _startY = 0f;
        private bool _isGrounded = false;
        private float _initialVelocity = 0f;
        private float _curGravity = 0f;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_gravityData.IsInitOnAwake)
            {
                Init();
            }
        }
        private void Update()
        {
            if (!_gravityData.FeetPoint)
            {
                return;
            }
            Debug.DrawRay(_gravityData.FeetPoint.position, Vector3.down * _gravityData.GroundCheckDistance, Color.red, 0.1f);
        }

        private void FixedUpdate()
        {
            Debug.DrawRay(transform.position + Vector3.one, transform.up * _checkCollisionDist, Color.red);

            if (!_gravityData.FeetPoint)
            {
                return;
            }
            if (IsGround(out Vector3 hitPoint))
            {
                if (_curGravity < _gravityData.Gravity)
                {
                    Init();
                }
                Vector3 pos = transform.position;
                if(_gravityData.CanUseSmoothGravity)
                {
                    pos.y = Mathf.Lerp(pos.y, hitPoint.y + _groundCheckDist , Time.fixedDeltaTime * _gravityData.SpeedToFeetPos);
                }
                else
                {
                    pos.y = Mathf.MoveTowards(pos.y, hitPoint.y + _groundCheckDist, Time.fixedDeltaTime * _gravityData.SpeedToFeetPos);
                }
                transform.position = pos;
                _startY = transform.position.y;
                _startTime = Time.time;
                return;
            }
            GravityProcess();
        }

        public void Init(GravityData? gravityData = null)
        {
            if(gravityData.HasValue) 
            {
                _gravityData = gravityData.Value;
            }
            _initCompleted = true;
            _curGravity = _gravityData.Gravity;
            _initialVelocity = 0f;
            _curSpeed = 0f;
            _curTime = 0f;
            _groundCheckDist = _gravityData.GroundCheckDistance;
            _startY = transform.position.y;
            _startTime = Time.time;
        }
        
        private void GravityProcess()
        {
            _checkCollisionPos = transform.position;
            _checkCollisionPos.y = _collider.bounds.max.y;

            _curTime = Time.time - _startTime;
            Vector3 pos = transform.position;
            float avgSpeed = (_curGravity * 0.5f * _curTime * _curTime) + _initialVelocity * _curTime;
            pos.y =  avgSpeed + _startY;
            transform.position = pos;
            _curSpeed = _initialVelocity + _curGravity * _curTime;
            if (_curSpeed < 0)
            {
                _curGravity -= Time.deltaTime * _gravityData.GravityMod;
                _groundCheckDist = _gravityData.GroundCheckDistance;
            }
            else if (Physics.Raycast(_checkCollisionPos, Vector3.up, out var hit, _checkCollisionDist))
            {
                //Y축 충돌하면 초기화
                if(hit.collider.gameObject.layer != gameObject.layer)
                {
                    _initialVelocity = 0f;
                    _startTime = Time.time;
                    _startY = transform.position.y;
                }
            }
        }
    
        public void AddForceUp(float force)
        {
            _initialVelocity = force;
            _startTime = Time.time;
            _startY = transform.position.y;
            _groundCheckDist = 0f;
        }

        private bool IsGround(out Vector3 hitPoint)
        {
            _isGrounded = UnityEngine.Physics.Raycast(_gravityData.FeetPoint.position, Vector3.down, out RaycastHit hit, _groundCheckDist, _gravityData.GroundLayerMask);
            hitPoint = hit.point;
            return _isGrounded;
        }


        public bool IsGrounded => _initialVelocity <= 0 && _isGrounded;
    }
}
