using UnityEngine;

public class GravityControl : MonoBehaviour
{
    [SerializeField]
    private Transform _feetPoint;
    [Header("Ground Check Distance")]
    [SerializeField]
    private float _groundCheckDistance = 1.2f;
    [Header("Ground Layer")]
    [SerializeField]
    private LayerMask _groundLayerMask;
    [Header("중력량")]
    [SerializeField]
    private float _gravity = -9.81f;
    [Header("중력 가중치")]
    [SerializeField] 
    private float _gravityThresold = 10f;
    [Header("떨어질때 목표 위치로 돌아가는 보간 속도")]
    [SerializeField] 
    private float _smoothSpeedToPos = 15f;

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
        Init();
    }
    private void Update()
    {
        Debug.DrawRay(_feetPoint.position, Vector3.down * _groundCheckDistance, Color.red, 0.1f);
    }

    private void FixedUpdate()
    {
        if (IsGround(out Vector3 hitPoint))
        {
            if (_curGravity < _gravity)
            {
                Init();
            }
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, hitPoint.y + _groundCheckDist , Time.fixedDeltaTime * _smoothSpeedToPos);
            transform.position = pos;
            _startY = transform.position.y;
            _startTime = Time.time;
            return;
        }
        GravityProcess();
    }

    private void Init()
    {
        _curGravity = _gravity;
        _initialVelocity = 0f;
        _curSpeed = 0f;
        _curTime = 0f;
        _groundCheckDist = _groundCheckDistance;
        _startY = transform.position.y;
        _startTime = Time.time;
    }
    private void GravityProcess()
    {
        _curTime = Time.time - _startTime;
        Vector3 pos = transform.position;
        float avgSpeed = (_curGravity * 0.5f * _curTime * _curTime) + _initialVelocity * _curTime;
        pos.y =  avgSpeed + _startY;
        transform.position = pos;
        _curSpeed = _initialVelocity + _curGravity * _curTime;
        if (_curSpeed < 0)
        {
            _curGravity -= Time.deltaTime * _gravityThresold;
            _groundCheckDist = _groundCheckDistance;
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
        _isGrounded=  Physics.Raycast(_feetPoint.position, Vector3.down, out RaycastHit hit, _groundCheckDist);
        hitPoint = hit.point;
        return _isGrounded;
    }


    public bool IsGrounded => _isGrounded;
}
