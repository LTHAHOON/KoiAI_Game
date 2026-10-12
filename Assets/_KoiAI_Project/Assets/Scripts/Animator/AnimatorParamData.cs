
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
        Direction
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
        [ShowIf(nameof(_movementAnimatorType), MovementAnimatorType.Speed)]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _walkParam;
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
        public int WalkParmID => IsNone(_walkParam) ? -1 : Animator.StringToHash(_walkParam);
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

        /// <summary>캐릭터 기준 이동 속도를 애니메이터에 전달합니다. 정지는 Vector3.zero를 전달합니다.</summary>
        public void SetMovementParameters(Animator animator, Vector3 localVelocity, float maxMoveSpeed)
        {
            if (!animator)
                return;

            Vector3 normalizedVelocity = Vector2.zero;
            if(maxMoveSpeed > 0f)
            {
                normalizedVelocity = Vector3.ClampMagnitude(localVelocity / maxMoveSpeed, 1f);
            }
            
            if (_movementAnimatorType == MovementAnimatorType.Direction)
            {
                if (!IsNone(_xMoveParam))
                {
                    animator.SetFloat(XMoveParamID, normalizedVelocity.x);
                }
                if (!IsNone(_zMoveParam))
                {
                    animator.SetFloat(ZMoveParamID, normalizedVelocity.z);
                }
            }
            else if (!IsNone(_walkParam))
            {
                animator.SetFloat(WalkParmID, normalizedVelocity.magnitude);
            }
        }
    }
}
