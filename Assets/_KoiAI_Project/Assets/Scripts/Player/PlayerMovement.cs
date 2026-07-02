using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using static KoiAI.Player.PlayerSFXAudioFeature;

namespace KoiAI.Player
{
    using KoiAI.AnimatorSystem;
    using KoiAI.Audio;
    using KoiAI.Physics;
    
    [Serializable]
    public class PlayerMovementExtensionData : PlayerFeatureExtensionData
    {
        #region 보정값 및 추가 이동 데이터
        
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

        #region 물리 데이터
        [Space(10)]
        [SerializeField]
        private GravityData _gravityData;
        [SerializeField]
        private RigidbodyData _rigidData;
        [SerializeField]
        private CapsuleColliderData _colliderData;
        #endregion

        public GravityData GravityData => _gravityData;
        public RigidbodyData RigidData => _rigidData;
        public CapsuleColliderData ColliderData => _colliderData;
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

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(GravityControl))]
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

        private GravityControl _gravityControl;
        private Rigidbody _rigidBody;
        private CapsuleCollider _capsuleCollider;
        private int _jumpCurCount = 0;
        private Vector3 _moveDir;
        private bool _isMoveStop;
        private UnityEngine.Camera _camera;
        private Vector3 _translation = Vector3.zero;
        private AnimatorData _animatorData;
        private AnimatorParamData _animParamData;
        public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Movement;

        public override void InitAutoInEnditor()
        {
            _mainSFXTarget = Owner.GetAudioSFXTarget(PlayerSFXAuidoProperty.Main);
            _moveSFXTarget = Owner.GetAudioSFXTarget(PlayerSFXAuidoProperty.Move);
        }

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
            //이동 데이터 초기화
            _valueData = valueData;
            _extensionValueData = extensionValueData;

            //물리 데이터 초기화
            InitPhysicsData(_extensionValueData.GravityData, _extensionValueData.ColliderData, _extensionValueData.RigidData);
            _camera = UnityEngine.Camera.main;
            
            playerIA.Player.Move.started += OnMove;
            playerIA.Player.Move.performed += OnMove;
            playerIA.Player.Move.canceled += OnMove;
            playerIA.Player.Jump.performed += OnJump;

            if(Owner.PlayerAnimatorData.IsValid())
            {
                //애니메이터 데이터 초기화
                _animatorData = Owner.PlayerAnimatorData;
                //애니메이터 파라미터 데이터 초기화
                _animParamData = Owner.PlayerAnimatorData.AnimParamData;
            }
            else
            {
                Debug.LogError("Error: PlayerAnimatorData is not valid.");
            }
        }
        public override void UpdateFeature()
        {
            if(!IsValid())
            {
                return;
            }
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

            #region 이동 키를 연속 누름으로 인한 애니메이션 및 소리 끊김 방지
            if (_isMoveStop)
            {
                if (_rigidBody.linearVelocity.sqrMagnitude <= _animatorData.StepAnimatorTresold)
                {
                    if (Owner.PlayerAnimatorData.IsValid() && _animParamData.WalkParmID > 0)
                    {
                        Owner.PlayerAnimator.SetBool(_animParamData.WalkParmID, false);
                    }
                    _isMoveStop = false;
                }
                if (_rigidBody.linearVelocity.sqrMagnitude <= _extensionValueData.StepAudioThresold)
                {
                    AudioManager.Instance.FadeStopSFX(_moveSFXTarget, 0.2f);
                    AudioManager.Instance.PlaySFX(_mainSFXTarget, _extensionValueData.StopStepAudioData, transform.position);
                }
            }
            #endregion

            if (_gravityControl.IsGrounded)
            {
                _jumpCurCount = 0;
            }
        }
    
        private void FixedUpdate()
        {
            if (!IsValid())
            {
                return;
            }
            if (_moveDir.sqrMagnitude > 0)
            {
                _rigidBody.linearVelocity = _translation;
            }
        }
    
        private void InitPhysicsData(GravityData? gravityData, CapsuleColliderData? colliderData, RigidbodyData? rigidData)
        {
            _gravityControl = GetComponent<GravityControl>();
            _rigidBody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            if (!gravityData.HasValue || !colliderData.HasValue || !rigidData.HasValue)
            {
                return;
            }
            var gravityValueData = gravityData.Value;
            var colliderValueData = colliderData.Value;
            var rigidValueData = rigidData.Value;

            gravityValueData.SetFeetPoint(Owner.CurrentPlayerSkin.FeetPoint);
            _gravityControl.Init(gravityValueData);
            _capsuleCollider.center = colliderValueData.Center;
            _capsuleCollider.radius = colliderValueData.Radius;
            _capsuleCollider.height = colliderValueData.Height;
            _capsuleCollider.direction = colliderValueData.Direction;
            _rigidBody.mass = rigidValueData.Mass;
            _rigidBody.linearDamping = rigidValueData.LinearDamping;
            _rigidBody.angularDamping = rigidValueData.AngularDamping;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 normalizedDir = context.ReadValue<Vector2>();
            _moveDir = new Vector3(normalizedDir.x, 0f, normalizedDir.y);
            _moveDir.Normalize();

            if (context.canceled)
            {
                _isMoveStop = true;

            }
            else
            {
                if (Owner.PlayerAnimatorData.IsValid() && _animParamData.WalkParmID > 0)
                {
                    Owner.PlayerAnimator.SetBool(_animParamData.WalkParmID, true);
                }
            }
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

        private bool IsValid() => _valueData != null && _extensionValueData != null && _rigidBody != null && _capsuleCollider != null;
    }
}
