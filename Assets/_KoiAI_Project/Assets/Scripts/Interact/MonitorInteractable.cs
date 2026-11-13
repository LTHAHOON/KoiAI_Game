using UnityEngine;

namespace KoiAI.Interact
{
    using DG.Tweening;
    using KoiAI.Player;

    public struct MonitorInteractContext
    {
        public int Password { get; set; }
    }

    public class MonitorInteractable : BaseInteractable<MonitorInteractContext>, IPlayerInteractable
    {
        [SerializeField]
        private CanvasGroup _monitorScreen;
        [SerializeField]
        private RectTransform _monitorHackingScreen;
        [SerializeField]
        private float _monitorScreen_ShowDuration = 0.3f;
        private Tweener _screenTweener;
        private bool _isOn = false;

        private void Awake()
        {
            _monitorScreen.alpha = 0f;
            _monitorScreen.gameObject.SetActive(false);
            _monitorHackingScreen.gameObject.SetActive(false);
            _screenTweener = _monitorScreen.DOFade(1f, _monitorScreen_ShowDuration)
                .SetEase(Ease.Linear)
                .OnPlay(() =>
                {
                    if (!_screenTweener.isBackwards)
                    {
                        _monitorScreen.gameObject.SetActive(true);
                    }
                })
                .OnComplete(() =>
                {
                    if (_screenTweener.isBackwards)
                    {
                    _monitorScreen.gameObject.SetActive(false);
                    }
                })
                .SetAutoKill(false)
                .Pause();
        }

        public void Interact(PlayerInteractor playerInteractor)
        {
            _isOn = !_isOn;

            if (_isOn)
            {
                _screenTweener.PlayForward();
            }
            else
            {
                _screenTweener.PlayBackwards();
            }
        }

        public override void Interact(MonitorInteractContext dataEvent)
        {
            if (dataEvent.Password != dataEvent.Password)
            {
                return;
            }

            base.Interact(dataEvent);
        }


    }
}
