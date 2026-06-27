using UnityEngine;

namespace KoiAI.Utilities.FadeInOutColor
{
    public abstract class FadeCondition : ScriptableObject
    {
        public abstract bool IsPossibleFadeIn(float fadeInOutCurTime);
        public abstract bool IsPossibleFadeOut(float fadeInOutCurTime);

    }
}
