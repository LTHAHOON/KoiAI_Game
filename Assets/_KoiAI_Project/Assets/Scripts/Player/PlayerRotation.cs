using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    public class PlayerRotation : PlayerFeature
    {
        [SerializeField]
        private float _lookSpeed = 10f;
        [SerializeField]
        private float _surfaceCheckDistance = 3f;
        [SerializeField]
        private LayerMask _surfaceLayerMask;

        public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Rotation;
        private SurfaceAngleFinder _surfaceAngleFinder;
        private UnityEngine.Camera _camera;
        private Vector2 _input = Vector2.zero;
        private Vector3 _targetAngle = Vector3.zero;
    
        public override void Init(PlayerInputAction playerIA)
        {
            _camera = UnityEngine.Camera.main;
            _surfaceAngleFinder = new(_surfaceCheckDistance);
            playerIA.Player.Move.started += OnRotation;
            playerIA.Player.Move.performed += OnRotation;
            playerIA.Player.Move.canceled += OnRotation;
        }
    
        public override void UpdateFeature()
        {
            if (!IsValid())
                return;
            Vector3 localForward = transform.InverseTransformDirection(transform.forward);
            _surfaceAngleFinder.TryGetLocalSurfaceAngle(out _targetAngle, transform, _surfaceLayerMask);
            _targetAngle.y = GetAngleWithAtan(_input);
        }

        public void FixedUpdate()
        {
            if (!IsValid())
                return;
            Quaternion quat = Quaternion.Euler(_targetAngle.x, _targetAngle.y, _targetAngle.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, quat, Time.deltaTime * _lookSpeed);

        }

        private float GetAngleWithAtan(Vector2 input)
        {
            Vector3 xDir = _camera.transform.right * input.x;
            Vector3 zDir = _camera.transform.forward * input.y;
            Vector3 dir = (xDir + zDir).normalized;
            dir.y = 0f;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return angle;
        }
        private float GetAngleWithDot(Vector2 input)
        {
            #region 내적을 이용하여 카메라 중심으로 Y 회전값 구하기
            Vector3 xDir = _camera.transform.right * input.x;
            Vector3 zDir = _camera.transform.forward * input.y;
            Vector3 dir = (xDir + zDir).normalized;
            dir.y = 0f;

            float dot = Vector3.Dot(transform.forward, dir);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            float crossY = Vector3.Cross(transform.forward, dir).y;
            //0~180도를 -180~180도로 확장
            if(crossY < 0f)
                angle = -angle;
        
            return angle + transform.eulerAngles.y;
            #endregion
        }

        public void OnRotation(InputAction.CallbackContext context)
        {
            if (context.started)
                return;
            if (context.performed)
            {
                SetInput(context.ReadValue<Vector2>());
            }
            if(context.canceled)
            {
                SetInput(Vector2.zero);
            }
        }

        public void SetInput(Vector2 input)
        {
            _input = input;
        }

        public void ConnectPlayerIA()
        {
            PlayerInputAction playerIA = Owner.PlayerIA;
            //playerIA 재연결
            playerIA.Disable();
            playerIA.Enable();
            playerIA.Player.Move.started += OnRotation;
            playerIA.Player.Move.performed += OnRotation;
            playerIA.Player.Move.canceled += OnRotation;
        }

        public void DisConnectPlayerIA()
        {
            PlayerInputAction playerIA = Owner.PlayerIA;
            playerIA.Player.Move.started -= OnRotation;
            playerIA.Player.Move.performed -= OnRotation;
            playerIA.Player.Move.canceled -= OnRotation;
        }
        private bool IsValid() => _input != Vector2.zero && _camera && _surfaceAngleFinder != null;
    }
}
