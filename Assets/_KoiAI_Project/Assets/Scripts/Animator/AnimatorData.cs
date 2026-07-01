using System;
using UnityEngine;

namespace KoiAI.AnimatorSystem
{
    [Serializable]
    public struct AnimatorData
    {
        [SerializeField]
        private Avatar _animatorAvatar;
        [SerializeField]
        private AnimatorParamData _animatorParamData;

        public readonly AnimatorParamData AnimParamData => _animatorParamData;
        public readonly RuntimeAnimatorController RuntimeAnimController => _animatorParamData.RuntimeAC;
        public readonly Avatar AnimatorAvatar => _animatorAvatar;

        public readonly bool IsValid() => _animatorAvatar != null && _animatorParamData != null && _animatorParamData.RuntimeAC != null;
    }
}
