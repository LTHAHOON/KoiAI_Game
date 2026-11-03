using UnityEngine;

namespace KoiAI.Utilities.FadeInOutColor
{
    /// <summary>
    /// 상속 시 하위 클래스 위에 Requirement(typeof()) 사용 권장
    /// </summary>
    public abstract class FadeInOutColor<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Fade 조건 데이터
        /// </summary>
        [SerializeField]
        private FadeCondition _fadeCondition;

        [SerializeField]
        private Color _baseColor = Color.white;
        [SerializeField]
        private float _fadeInSpeed;
        [SerializeField]
        private float _fadeOutSpeed;
        [SerializeField]
        private bool _canPlayOnAwake;
        [SerializeField]
        private bool _canPlayOnEnable;
        [SerializeField]
        private bool _isLoop = false;

        private T _uiComponent;
        private bool _isEndInit = false;
        private bool _isPlaying = false;
        private float _fadeInOutCurTime = 0f;
        private void Awake()
        {
            _uiComponent = GetComponent<T>();
            Init();
            _isEndInit = true;
            if (_canPlayOnAwake)
            {
                PlayFadeInOut();
            }
        }

        private void Update()
        {
            if (_isPlaying)
            {
                UpdateFadeInOut();
            }
            else if(_isLoop)
            {
                PlayFadeInOut();
            }
        }

        private void OnEnable()
        {
            if (_canPlayOnEnable)
            {
                PlayFadeInOut();
            }
        }

        private void OnDisable()
        {
            StopFadeInOut();
            _fadeInOutCurTime = 0f;
            SetCurrentColor(_baseColor);
        }

        public void PlayFadeInOut()
        {
            _isPlaying = true;
        }

        public void StopFadeInOut()
        {
            _isPlaying = false;
        }

        private void UpdateFadeInOut()
        {
            if (!_isEndInit)
            {
                return;
            }

            if (_fadeCondition == null)
            {
                _isPlaying = false;
                return;
            }

            _fadeInOutCurTime += Time.deltaTime;
            if (_fadeCondition.IsPossibleFadeIn(_fadeInOutCurTime) && _fadeInSpeed > 0f)
            {
                SetFadeColor(_fadeInSpeed);
            }
            else if (_fadeCondition.IsPossibleFadeOut(_fadeInOutCurTime) && _fadeOutSpeed > 0f)
            {
                SetFadeColor(-_fadeOutSpeed);
            }
            else if (_fadeCondition.IsComplete(_fadeInOutCurTime))
            {
                _fadeInOutCurTime = 0f;
                _isPlaying = _isLoop;
            }
        }

        private void SetFadeColor(float fadeSpeed)
        {
            Color curColor = GetCurrentColor();
            float alpha = curColor.a + (Time.deltaTime * fadeSpeed);
            curColor.a = Mathf.Clamp01(alpha);

            SetCurrentColor(curColor);
        }

        protected abstract void Init();
        protected abstract void SetCurrentColor(Color color);
        protected abstract Color GetCurrentColor();
        protected Color GetBaseColor() => _baseColor;

        protected T GetUIComponent() => _uiComponent;
    }
}
