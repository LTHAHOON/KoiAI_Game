using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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

    private float _curTime = 0;
    public override void Init()
    {
        _curTime = 0;
    }

    public override bool IsPossibleFadeIn()
    {
        if (IsPossbileFadeInOut())
        {
            bool isPossibleFadeIn = IsStartFadeIn();
            return isPossibleFadeIn;
        }
        return false;
    }

    public override bool IsPossibleFadeOut()
    {
        if(IsPossbileFadeInOut())
        {
            bool isPossibleFadeOut = IsStartFadeOut();
            return isPossibleFadeOut;
        }
        return false;
    }

    private bool IsPossbileFadeInOut()
    {
        _curTime += Time.deltaTime;
        bool isPossibleFade = !IsTimeOut();
        return isPossibleFade;
    }

    private bool IsTimeOut() => _curTime >= _duration;
    private bool IsStartFadeIn() => _curTime >= _fadeInStartTime && _curTime <= _fadeInEndTime;

    private bool IsStartFadeOut() => _curTime >= _fadeOutStartTime && _curTime <= _fadeOutEndTime;

    public float FadeInEndTime => _fadeInEndTime;
    public float FadeOutEndTime => _fadeOutEndTime;
}
