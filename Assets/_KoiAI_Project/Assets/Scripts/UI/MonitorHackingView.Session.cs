using System;
using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoiAI.Pool;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.UI
{
    public partial class MonitorHackingView
    {
        private void ResetHackingSession()
        {
            StopHackingSession();
            ClearHackingTexts();
            _restartHackingButton.onClick.RemoveAllListeners();
            foreach(MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.EndShuffleTextLoop();
            }
            _completedCount = 0;
            _curHackingBarIndex = 0;
            _restartHackingCanvasGroup.DOKill();
            _restartHackingCanvasGroup.alpha = 0f;
            _restartHackingButton.interactable = false;
            _loadingRewardProgressText.text = string.Empty;
            _loadingRewardCavasGroup.DOKill();
            _loadingRewardCavasGroup.alpha = 0f;
            _timeText.SetTextFormat("{0:F2}", _timeLimit);
            _timerGuageImage.fillAmount = 1f;
            _timerCompass.localEulerAngles = Vector3.zero;
            ResetSignals();
            foreach (MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.ResetText();
            }
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex].transform);
            _hackingBar.anchoredPosition = new(0f, _yOffset_HackingBar);
        }

        private void RestartHacking()
        {
            RestartHackingAsync().Forget();
        }

        private async UniTask RestartHackingAsync()
        {
            _restartHackingButton.interactable = false;
            await StopHackingSessionAsync();
            ResetHackingSession();
            _restartHackingButton.onClick.RemoveAllListeners();
            _restartHackingButton.onClick.AddListener(RestartHacking);
            StartHackingSession();
        }

        private void StartHackingSession()
        {
            StopHackingSession();
            _restartHackingButton.interactable = false;
            _startHackingCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            int sessionVersion = ++_hackingSessionVersion;
            _hackingSessionTimedOut = false;
            _hackingSessionTimedOutVersion = -1;
            _hackingSessionCompletion = new UniTaskCompletionSource();
            StartMonitorHacking(_startHackingCts.Token, sessionVersion, _hackingSessionCompletion).Forget();
        }

        private async UniTask StopHackingSessionAsync()
        {
            CancellationTokenSource cts = _startHackingCts;
            UniTaskCompletionSource sessionCompletion = _hackingSessionCompletion;
            ++_hackingSessionVersion;
            _startHackingCts = null;
            _hackingSessionCompletion = null;

            cts?.Cancel();
            try
            {
                if (sessionCompletion != null)
                {
                    await sessionCompletion.Task;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts?.Dispose();
            }
        }


        private void StopHackingSession()
        {
            ++_hackingSessionVersion;
            _startHackingCts?.Cancel();
            _startHackingCts?.Dispose();
            _startHackingCts = null;
        }

        private void ShowRestartButton()
        {
            _restartHackingButton.interactable = true;
            _restartHackingCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);
        }
    }
}
