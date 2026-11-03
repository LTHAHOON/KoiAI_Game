using UnityEngine;

namespace KoiAI.Utilities.FadeInOutColor
{
    [CreateAssetMenu(fileName = "new TimeFadeCondition", menuName = "FadeCondition/TimeFadeCondition")]
    public class TimeFadeCondition : FadeCondition
    {
        [SerializeField]
        private float _duration = 1.0f;
        [SerializeField]
        private float _fadeInStartTime;
        [SerializeField]
        private float _fadeInEndTime;
        [SerializeField]
        private float _fadeOutStartTime;
        [SerializeField]
        private float _fadeOutEndTime;

        public override bool IsPossibleFadeIn(float curTime)
        {
            return IsPossibleTime(curTime) && IsStartFadeIn(curTime);
        }

        public override bool IsPossibleFadeOut(float curTime)
        {
            return IsPossibleTime(curTime) && IsStartFadeOut(curTime);
        }

        private bool IsPossibleTime(float curTime) => curTime <= _duration;
        private bool IsStartFadeIn(float curTime) => curTime >= _fadeInStartTime && curTime <= _fadeInEndTime;

        private bool IsStartFadeOut(float curTime) => curTime >= _fadeOutStartTime && curTime <= _fadeOutEndTime;

        public override bool IsComplete(float curTime) => curTime >= _duration;

        public float FadeInEndTime => _fadeInEndTime;
        public float FadeOutEndTime => _fadeOutEndTime;
    }
}
