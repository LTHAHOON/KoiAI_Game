using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace KoiAI.AnimatorSystem
{
    public enum MovementAnimatorType
    {
        Speed,
        Direction,
        Bool
    }

    [CreateAssetMenu(fileName = "new AnimatorParamData", menuName = "KoiAI/AnimatorSystem/AnimatorParamData")]
    public class AnimatorParamData : ScriptableObject
    {
        [SerializeField]
        private RuntimeAnimatorController _runtimeAnimatorController;

        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _idleParam;
        [SerializeField]
        [BoxGroup("이동 애니메이션")]
        private MovementAnimatorType _movementAnimatorType = MovementAnimatorType.Speed;
        [SerializeField]
        [Min(0f)]
        [HideIf(nameof(_movementAnimatorType), MovementAnimatorType.Bool)]
        [BoxGroup("이동 애니메이션")]
        [Tooltip("이동 파라미터 보간 시간(초). 0이면 즉시 반영합니다.")]
        private float _movementDampTime;

        public float MovementDampTime => _movementDampTime;

        private bool UsesSingleMovementParameter => _movementAnimatorType == MovementAnimatorType.Speed || _movementAnimatorType == MovementAnimatorType.Bool;

        [SerializeField]
        [ShowIf(nameof(UsesSingleMovementParameter))]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _moveParam;
        [SerializeField]
        [ShowIf(nameof(_movementAnimatorType), MovementAnimatorType.Direction)]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _xMoveParam = "None";
        [SerializeField]
        [ShowIf(nameof(_movementAnimatorType), MovementAnimatorType.Direction)]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _zMoveParam = "None";
        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _jumpParam;
        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _attack01_Param;
        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _attack02_Param;

        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _act1_Param;
        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _act2_Param;

        private readonly List<string> _animatorParamList = new();

#if UNITY_EDITOR

        public void OnValueChanged()
        {
            EditorUtility.SetDirty(this);
        }

        public List<string> GetAnimatorParamList()
        {
            _animatorParamList.Clear();
            if (!_runtimeAnimatorController)
            {
                _animatorParamList.Add("AnimatorController를 먼저 등록해주세요.");
                return _animatorParamList;
            }
            if(_animatorParamList.Count <= 0)
            {
                if (_runtimeAnimatorController is AnimatorController animatorController)
                {
                    int paramCount = animatorController.parameters.Length;
                    if (paramCount <= 0)
                    {
                        _animatorParamList.Add("해당 AnimatorController에는 Parameter가 없습니다.");
                    }
                    else
                    {
                        _animatorParamList.Add("None");
                        for (int i = 0; i < paramCount; i++)
                        {
                            _animatorParamList.Add(animatorController.parameters[i].name);
                        }
                    }
                }
            }

            return _animatorParamList;
        }
#endif

        public RuntimeAnimatorController RuntimeAC => _runtimeAnimatorController;
        private bool IsNone(string param) => string.IsNullOrEmpty(param) || param == "None";

        /// <summary> -1일 경우 None</summary>
        public int IdleParmID => IsNone(_idleParam) ? -1 : Animator.StringToHash(_idleParam);
        /// <summary> -1일 경우 None</summary>
        public int MoveParmID => IsNone(_moveParam) ? -1 : Animator.StringToHash(_moveParam);
        /// <summary> -1일 경우 None</summary>
        public int XMoveParamID => IsNone(_xMoveParam) ? -1 : Animator.StringToHash(_xMoveParam);
        /// <summary> -1일 경우 None</summary>
        public int ZMoveParamID => IsNone(_zMoveParam) ? -1 : Animator.StringToHash(_zMoveParam);
        /// <summary> -1일 경우 None</summary>
        public int JumpParmID => IsNone(_jumpParam) ? -1 : Animator.StringToHash(_jumpParam);
        /// <summary> -1일 경우 None</summary>
        public int Act1ParamID => IsNone(_act1_Param) ? -1 : Animator.StringToHash(_act1_Param);
        /// <summary> -1일 경우 None</summary>
        public int Act2ParamID => IsNone(_act2_Param) ? -1 : Animator.StringToHash(_act2_Param);

        /// <summary>
        /// Exit에서만 호출하여 정지값까지 보간합니다. 새 이동이 시작되면 종료합니다.
        /// </summary>
        public async UniTask StopMovementParametersAsync(Animator animator, CancellationToken cancellationToken)
        {
            if (!animator || !cancellationToken.CanBeCanceled)
            {
                return;
            }

            if (_movementAnimatorType == MovementAnimatorType.Bool)
            {
                SetMovementParameters(animator, Vector3.zero, 0f);
                return;
            }

            int speedParamId1 = _movementAnimatorType == MovementAnimatorType.Direction ? XMoveParamID : MoveParmID;
            int speedParamId2 = _movementAnimatorType == MovementAnimatorType.Direction ? ZMoveParamID : -1;

            bool StopParameter(int paramId)
            {
                if (paramId == -1)
                {
                    return true;
                }

                SetMovementParameters(animator, Vector3.zero, 0f);
                float currentSpeed = animator.GetFloat(paramId);
                bool stopped = Mathf.Abs(currentSpeed) <= 0.001f;
                return stopped;
            }

            while (animator && animator.isActiveAndEnabled)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (StopParameter(speedParamId1) & StopParameter(speedParamId2))
                {
                    return;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        /// <summary>
        /// Movement 설정 함수
        /// </summary>
        public void SetMovementParameters(Animator animator, Vector3 localVelocity, float maxMoveSpeed)
        {
            if (!animator)
            {
                return;
            }

            Vector3 normalizedVelocity = maxMoveSpeed > 0f
                ? Vector3.ClampMagnitude(localVelocity / maxMoveSpeed, 1f)
                : Vector3.zero;

            switch(_movementAnimatorType)
            {
                case MovementAnimatorType.Bool:
                    if(!IsNone(_moveParam))
                    {
                        animator.SetBool(MoveParmID, normalizedVelocity.magnitude > 0.0001f);
                    }
                    break;
                case MovementAnimatorType.Speed:
                    if(!IsNone(_moveParam))
                    {
                        animator.SetFloat(MoveParmID, normalizedVelocity.magnitude, _movementDampTime, Time.deltaTime);
                    }
                    break;
                case MovementAnimatorType.Direction:
                    if (!IsNone(_xMoveParam))
                    {
                        animator.SetFloat(XMoveParamID, normalizedVelocity.x, _movementDampTime, Time.deltaTime);
                    }
                    if (!IsNone(_zMoveParam))
                    {
                        animator.SetFloat(ZMoveParamID, normalizedVelocity.z, _movementDampTime, Time.deltaTime);
                    }
                    break;
            }
        }
    }
}
