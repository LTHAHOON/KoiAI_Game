using DG.Tweening;
using R3;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.UI
{
    
    [Serializable]
    public struct UIScaleTransitionData
    {
        [SerializeField]
        private Ease _easingType;
        [SerializeField]
        private float _duration;
        [SerializeField]
        private Vector2 _minScale;
        [SerializeField]
        private Vector2 _maxScale;

        public UIScaleTransitionData(Ease easingType, float duration, Vector2 minScale, Vector2 maxScale)
        {
            _easingType = easingType;
            _duration = duration;
            _minScale = minScale;
            _maxScale = maxScale;
        }

        public Ease EasingType => _easingType;
        public float Duration => _duration;
        public Vector2 MinScale => _minScale;
        public Vector2 MaxScale => _maxScale;
    }

    public class UIScaleTransition
    {
        private UIScaleTransitionData _scaleTransitionData;
        private VisualElement _uiTarget;
        private GameObject _caller;
        
        public UIScaleTransition(GameObject caller, VisualElement uiTarget, UIScaleTransitionData scaleTransitionData)
        {
            if(!caller || uiTarget == null)
            {
                return;
            }
            _caller = caller;
            _uiTarget = uiTarget;
            _scaleTransitionData = scaleTransitionData;
        }
        public UIScaleTransition(GameObject caller, Ease easingType,VisualElement uiTarget, float duration, Vector2 minScale, Vector2 maxScale)
        {
            if (!caller || uiTarget == null)
            {
                return;
            }
            _caller = caller;
            _uiTarget = uiTarget;
            _scaleTransitionData = new(easingType, duration, minScale, maxScale);
        }

        public void ActivateTransition()
        {
            if(_uiTarget == null)
            {
                return;
            }
            Vector2 minScale = _scaleTransitionData.MinScale;

            DOTween.To(
                () => minScale,
                x => { minScale = x; _uiTarget.style.scale = x; },
                _scaleTransitionData.MaxScale,
                _scaleTransitionData.Duration
                )
                .SetId(_uiTarget)
                .SetEase(_scaleTransitionData.EasingType)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(_caller, LinkBehaviour.KillOnDestroy);
                
        }

    }
}
