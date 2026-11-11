
using DG.Tweening;
using KoiAI.Utilities;
using NaughtyAttributes;
using UnityEngine;

namespace KoiAI
{
    public sealed class DitherableObject : MonoBehaviour
    {
        [SerializeField]
        private Renderer[] _renderers;

        [SerializeField, Min(0f)]
        private float _fadeDuration = 0.25f;

        private Tweener _fadeTween;

        private int _alphaPropertyId;
        private float _alpha = 1f;
        private float _targetAlpha = 1f;
        private bool _initialized;

        [Button("Set Renderers")]
        public void OnSetRenderers()
        {
            _renderers = transform.GetComponentsInChildren<Renderer>(true);
        }

        public void Initialize(int alphaPropertyId)
        {
            if (_initialized)
                return;

            _alphaPropertyId = alphaPropertyId;

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }

            // Tween은 최초 한 번만 생성
            _fadeTween = DOTween.To(
                    () => _alpha,
                    SetAlpha,
                    1f,
                    _fadeDuration)
                .SetEase(Ease.Linear)
                .SetAutoKill(false)
                .Pause();

            _initialized = true;
        }

        public void SetOccluded(float targetAlpha)
        {
            if (!_initialized)
            {
                return;
            }

            targetAlpha = Mathf.Clamp01(targetAlpha);
            if (Mathf.Abs(_targetAlpha - targetAlpha) < 0.01f)
            {
                return;
            }

            _targetAlpha = targetAlpha;

            if (_fadeDuration <= 0f)
            {
                _fadeTween.Pause();
                SetAlpha(targetAlpha);
                return;
            }

            // 현재 알파에서 새로운 목표값으로 이동
            _fadeTween.ChangeEndValue(targetAlpha, _fadeDuration, true);

            _fadeTween.Restart();
        }

        private void SetAlpha(float value)
        {
            _alpha = value;

            for (int i = 0; i < _renderers.Length; i++)
            {
                MPBSystem.ChangeMaterialProperty(_renderers[i], _alphaPropertyId, value);
            }
        }
    }
}
