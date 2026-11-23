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
            ClearHackingTexts();
            _restartHackingButton.onClick.RemoveAllListeners();
            StopHackingSession();
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
            _restartHackingButton.onClick.RemoveAllListeners();
            _restartHackingButton.onClick.AddListener(RestartHacking);
            if (_hackingTextPool == null)
            {
                ulong entityID = _hackingTextPrefab.GetEntityULongID();
                PoolManager.Instance.AddPool(entityID, _hackingTextPrefab, _hackingPoolSize, PoolName.MonitorHacking);
                PoolManager.Instance.TryGetPool(entityID, out _hackingTextPool);
            }

            foreach(MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.StartShuffleTextLoop();
            }
            _startHackingCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            StartMonitorHacking(_startHackingCts.Token).Forget();
        }


        private void StopHackingSession()
        {
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
