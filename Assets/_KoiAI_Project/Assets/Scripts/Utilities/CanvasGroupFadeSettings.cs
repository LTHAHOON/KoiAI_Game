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
}
