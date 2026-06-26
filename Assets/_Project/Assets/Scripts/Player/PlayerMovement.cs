using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : PlayerFeature
{
    [SerializeField]
    private AudioData _stepAuidoData;
    [SerializeField]
    private AudioData _stopStepAudioData;
    [SerializeField]
    private AudioData _jumpAudioData;

    [SerializeField]
    private float _stepAudioThresold;
    [SerializeField]
    private AudioSFXTarget _mainSFXTarget;
    [SerializeField]
    private AudioSFXTarget _moveSFXTarget;

    [SerializeField]
    private Rigidbody _rigidBody;
    [SerializeField]
    private GravityControl _gravityControl;
    [SerializeField]
    private float _moveSpeed = 10f;
    [SerializeField]
    private float _jumpForce = 10f;
    [SerializeField]
    private int _jumpMaxCount = 1;


    private int _jumpCurCount = 1;
    private Vector3 _moveDir;
    private bool _isMoveStop;
    private Camera _camera;
    private Vector3 _translation = Vector3.zero;
    public override void Init(PlayerInputAction playerIA)
    {
        _camera = Camera.main;
        playerIA.Player.Move.started += OnMove;
        playerIA.Player.Move.performed += OnMove;
        playerIA.Player.Move.canceled += OnMove;
        playerIA.Player.Jump.performed += OnJump;
    }
    public override void UpdateFeature()
    {
        if (_moveDir.sqrMagnitude > 0)
        {
            if (!_moveSFXTarget.IsPlayingSFX())
            {
                AudioManager.Instance.PlaySFX(_moveSFXTarget, _stepAuidoData, transform.position);
            }
            Vector3 cameraRight = _camera.transform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();
            Vector3 cameraForward = _camera.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 xDir = cameraRight * _moveDir.x;
            Vector3 zDir = cameraForward * _moveDir.z;
            Vector3 dir = (xDir + zDir).normalized;
            dir.y = 0f;
            _translation = _moveSpeed * dir;
            AudioManager.Instance.UpdateSFXAudioPos(_mainSFXTarget, transform.position);
            AudioManager.Instance.UpdateSFXAudioPos(_moveSFXTarget, transform.position);

        }

        #region 키를 연속 누름으로 인한 소리 끊김 방지
        if (_isMoveStop)
        {
            if (_rigidBody.linearVelocity.sqrMagnitude <= _stepAudioThresold)
            {
                Debug.Log(_rigidBody.linearVelocity.sqrMagnitude);
                AudioManager.Instance.FadeStopSFX(_moveSFXTarget, 0.2f);
                AudioManager.Instance.PlaySFX(_mainSFXTarget, _stopStepAudioData, transform.position);
                _isMoveStop = false;
            }
        }
        #endregion

        if (_gravityControl.IsGrounded)
        {
            _jumpCurCount = 1;
        }
    }
    
    private void FixedUpdate()
    {
        if (_moveDir.sqrMagnitude > 0)
        {
            _rigidBody.linearVelocity = _translation;
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _isMoveStop = true;

        }
        Vector2 normalizedDir = context.ReadValue<Vector2>();
        _moveDir = new Vector3(normalizedDir.x, 0f, normalizedDir.y);
        _moveDir.Normalize();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if(_jumpCurCount < _jumpMaxCount)
            {
                ++_jumpCurCount;
                _gravityControl.AddForceUp(_jumpForce);
                AudioManager.Instance.PlaySFX(_mainSFXTarget, _jumpAudioData, transform.position);
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            Debug.Log("Mouse Click");
        }
    }
}
