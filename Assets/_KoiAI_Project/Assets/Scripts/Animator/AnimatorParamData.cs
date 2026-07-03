
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace KoiAI.AnimatorSystem
{
    [CreateAssetMenu(fileName = "new AnimatorParamData", menuName = "KoiAI/AnimatorSystem/AnimatorParamData")]
    public class AnimatorParamData : ScriptableObject
    {
        [SerializeField]
        private RuntimeAnimatorController _runtimeAnimatorController;

        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _walkParam;
        [SerializeField]
        [BoxGroup("애니메이터 파라미터(없으면 None)")]
        [Dropdown("GetAnimatorParamList")]
        [OnValueChanged("OnValueChanged")]
        private string _jumpParam;

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
        private bool IsNone(string param) => param == "None";
        /// <summary> -1일 경우 None</summary>
        public int WalkParmID => IsNone(_walkParam) ? -1 : Animator.StringToHash(_walkParam);
        /// <summary> -1일 경우 None</summary>
        public int JumpParmID => IsNone(_jumpParam) ? -1 : Animator.StringToHash(_jumpParam);
    }
}
