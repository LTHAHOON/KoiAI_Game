using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    using KoiAI.Audio;
    using KoiAI.Gravity_Physics;
    
    [Serializable]
    public class PlayerMovementExtensionData : PlayerFeatureExtensionData
    {
        #region 가중치 및 추가 데이터
        
        [SerializeField]
        private AudioData _stepAuidoData;
        [SerializeField]
        private AudioData _stopStepAudioData;
        [SerializeField]
        private AudioData _jumpAudioData;
        [SerializeField]
        private float _moveSpeedMod = 10f;
        [SerializeField]
        private float _jumpForceMod = 10f;
        [SerializeField]
        private int _jumpMaxCountMod = 1;
        [SerializeField]
        private float _stepAudioThresold;

        #endregion
        
        public AudioData StepAuidoData => _stepAuidoData;
        public AudioData StopStepAudioData => _stopStepAudioData;
        public AudioData JumpAudioData => _jumpAudioData;
        public float MoveSpeedMod => _moveSpeedMod;
        public float JumpForceMod => _jumpForceMod;
        public int JumpMaxCountMod => _jumpMaxCountMod;
        public float StepAudioThresold => _stepAudioThresold;   

    }
    
    [Serializable]
    public class PlayerMovementValueData : PlayerFeatureValueData
    {
        #region 이동 데이터
        
        [SerializeField]
        private float _moveSpeed = 10f;
        [SerializeField]
        private float _jumpForce = 10f;
        [SerializeField]
        private int _jumpMaxCount = 1;

        #endregion
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public int JumpMaxCount => _jumpMaxCount;
    }
    
    public class PlayerMovement : PlayerFeature
    {
        [ReadOnly]
        [SerializeField]
        private PlayerMovementValueData _valueData;
        [ReadOnly]
        [SerializeField]
        private PlayerMovementExtensionData _extensionValueData;
        [SerializeField]
        private AudioSFXTarget _mainSFXTarget;
        [SerializeField]
        private AudioSFXTarget _moveSFXTarget;
        [SerializeField]
        private Rigidbody _rigidBody;
        [SerializeField]
        private GravityControl _gravityControl;
        
        private int _jumpCurCount = 1;
        private Vector3 _moveDir;
        private bool _isMoveStop;
        private UnityEngine.Camera _camera;
        private Vector3 _translation = Vector3.zero;
        public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Movement;

        public override void Init(PlayerInputAction playerIA, PlayerFeatureValueData playerFeatureValueData = null, 
            PlayerFeatureExtensionData playerFeatureExtensionData = null)
        {
            if (playerIA == null)
            {
                return;
            }
            if (playerFeatureValueData is not PlayerMovementValueData valueData ||
                playerFeatureExtensionData is not PlayerMovementExtensionData extensionValueData)
            {
                return;
            }
            
            _valueData = valueData;
            _extensionValueData = extensionValueData;
            _camera = UnityEngine.Camera.main;
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
                    AudioManager.Instance.PlaySFX(_moveSFXTarget, _extensionValueData.StepAuidoData, transform.position);
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
                _translation = (_valueData.MoveSpeed + _extensionValueData.MoveSpeedMod) * dir;
                AudioManager.Instance.UpdateSFXAudioPos(_mainSFXTarget, transform.position);
                AudioManager.Instance.UpdateSFXAudioPos(_moveSFXTarget, transform.position);

            }

            #region 키를 연속 누름으로 인한 소리 끊김 방지
            if (_isMoveStop)
            {
                if (_rigidBody.linearVelocity.sqrMagnitude <= _extensionValueData.StepAudioThresold)
                {
                    AudioManager.Instance.FadeStopSFX(_moveSFXTarget, 0.2f);
                    AudioManager.Instance.PlaySFX(_mainSFXTarget, _extensionValueData.StopStepAudioData, transform.position);
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
                if(_jumpCurCount < (_valueData.JumpMaxCount + _extensionValueData.JumpMaxCountMod))
                {
                    ++_jumpCurCount;
                    _gravityControl.AddForceUp(_valueData.JumpForce + _extensionValueData.JumpForceMod);
                    AudioManager.Instance.PlaySFX(_mainSFXTarget, _extensionValueData.JumpAudioData, transform.position);
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
}
