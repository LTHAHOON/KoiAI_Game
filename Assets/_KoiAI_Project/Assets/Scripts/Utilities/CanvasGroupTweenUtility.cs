using System;
using DG.Tweening;
using UnityEngine;

namespace KoiAI.Utilities
{
    [Serializable]
    public struct CanvasGroupFadeSettings
    {
        [SerializeField, Min(0f)] 
        private float _duration;
        [SerializeField]
        private Ease _ease;
        [SerializeField]
        private bool _useUnscaledTime;

        public CanvasGroupFadeSettings(float duration, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _duration = duration;
            _ease = ease;
            _useUnscaledTime = useUnscaledTime;
        }

        public readonly float Duration => _duration;
        public readonly Ease Ease => _ease;
        public readonly bool UseUnscaledTime => _useUnscaledTime;
    }

    public static class CanvasGroupTweenUtility
    {
        public static Tweener CreateFade(CanvasGroup canvasGroup, bool fadeIn, float duration, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            return CreateFade(canvasGroup, fadeIn, new CanvasGroupFadeSettings(duration, ease, useUnscaledTime));
        }

        public static Tweener CreateFade(CanvasGroup canvasGroup, bool fadeIn, CanvasGroupFadeSettings settings)
        {
            if (canvasGroup == null)
            {
                throw new ArgumentNullException(nameof(canvasGroup));
            }

            canvasGroup.alpha = fadeIn ? 0f : 1f;
            
            Tweener tweener = canvasGroup.DOFade(fadeIn ? 1f : 0f, Mathf.Max(0f, settings.Duration))
                .SetEase(settings.Ease)
                .SetUpdate(settings.UseUnscaledTime)
                .OnPlay(() => canvasGroup.gameObject.SetActive(true))
                .OnComplete(() =>
                {
                    if (!fadeIn)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }
                })
                .OnRewind(() =>
                {
                    if (fadeIn)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }
                })
                .SetAutoKill(false)
                .Pause();

            canvasGroup.gameObject.SetActive(!fadeIn);
            return tweener;
        }
    }
}
