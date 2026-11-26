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
    public partial class MonitorHackingWindow
    {
        private void ResetHackingSession()
        {
            SetActiveRestartButton(false);
            StopHackingSession();
            ClearHackingTexts();
            foreach(MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.EndShuffleTextLoop();
            }
            _completedCount = 0;
            _curHackingBarIndex = 0;
            _loadingRewardProgressText.text = string.Empty;
            _timeText.SetTextFormat("{0:F2}", _timeLimit);
            _timerGuageImage.fillAmount = 1f;
            _timerCompass.localEulerAngles = Vector3.zero;
            ResetSignals();
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex].transform);
            _hackingBar.anchoredPosition = new(0f, _yOffset_HackingBar);
        }

        private void RestartHacking()
        {
            ResetHackingSession();
            StartHackingSession();
        }

        private void StartHackingSession()
        {
            foreach (MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.StartShuffleTextLoop();
            }
            _startHackingCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            StartMonitorHacking(_startHackingCts.Token).Forget();
        }



        private void StopHackingSession()
        {
            if(_startHackingCts is null)
            {
                return;
            }

            _startHackingCts.Cancel();
            _startHackingCts.Dispose();
            _startHackingCts = null;
        }

        private void SetActiveRestartButton(bool isActive)
        {
            if(isActive)
            {
                _restartHackingButton.interactable = true;
                _restartHackingCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);
            }
            else
            {
                _restartHackingButton.interactable = false;
                _restartHackingCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear);
            }
            
        }
    }
}
