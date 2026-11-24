using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    public partial class MonitorHackingView
    {
        private void ResetSignals()
        {
            _curSignal = _signalImages.Count;
            foreach (Image signalImage in _signalImages)
            {
                if (signalImage != null)
                {
                    signalImage.fillAmount = 1f;
                }
            }
        }

        private void ConsumeSignal()
        {
            if (_curSignal <= 0)
            {
                return;
            }
            --_curSignal;
            Image signalImage = _signalImages[_curSignal];
            if (signalImage != null)
            {
                if (_curSignal == 0)
                {
                    signalImage.DOFillAmount(0f, _signalDrainDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        _hackingFailed = true;
                        StopHackingSession();
                    });
                    return;
                }
                signalImage.DOFillAmount(0f, _signalDrainDuration).SetEase(Ease.OutQuad);
            }
        }

        private async UniTask StartLoadingRewardProgress()
        {
            Tween loadingTween = null;
            try
            {
                loadingTween = _loadingRewardCavasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear).SetAutoKill(false);
                while (!loadingTween.IsComplete())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                using Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
                for (int i = 0; i < TARGET_LOADING_PROGRESS.Length; ++i)
                {
                    sb.Append(TARGET_LOADING_PROGRESS[i]);
                    _loadingRewardProgressText.SetTextFormat("{0}", sb);
                    float loadingDelay = UnityEngine.Random.Range(0.05f, _maxLoadingDelay);
                    await UniTask.Delay(TimeSpan.FromSeconds(loadingDelay));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                loadingTween.OnRewind(() => loadingTween.Kill());
                loadingTween?.Rewind();
            }
        }

    }
}
