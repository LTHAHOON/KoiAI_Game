using TMPro;
using UnityEngine;

namespace KoiAI.UI
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using KoiAI.Pool;
    using KoiAI.Utilities;

    public class MonitorHackingView : PopUpWindow_Canvas
    {
        [SerializeField]
        private float _startHackingDelay = 1f;
        [SerializeField]
        private float _hackingTextDelay = 1.5f;
        [SerializeField]
        private float _minHackingTextDelay = 1f;
        [SerializeField]
        private float _maxHackingTextDelay = 2f;
        [SerializeField]
        private float _hackingTextSpeed = 500f;
        [SerializeField]
        private CanvasGroup _monitorHackingScreen;
        [SerializeField]
        private CanvasGroupFadeSettings _fadeSettings = new(0.3f);
        [SerializeField]
        private TMP_Text _hackingTextPrefab;
        [SerializeField]
        private PoolSize _hackingPoolSize;
        [SerializeField]
        private RectTransform _hackingBar;
        [SerializeField]
        private float _yOffset_HackingBar;
        [SerializeField]
        private RectTransform[] _hackingSlots;
        [SerializeField]
        private RectTransform[] _hackingTexts_Pos;

        private int _curHackingBarIndex;
        private Pool<TMP_Text> _hackingTextPool;
        private CancellationTokenSource _startHackingCts;
        private Dictionary<RectTransform, List<TMP_Text>> _dicCurHackingTexts;

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            if (_hackingSlots.Length <= 0)
            {
                Debug.LogError("ERROR: HackingSlot이 비었습니다.");
                return;
            }
            if (_hackingTexts_Pos.Length <= 0)
            {
                Debug.LogError("ERROR: HackingTexts이 비었습니다.");
                return;
            }
            _dicCurHackingTexts = new();
            for (int i = 0; i < _hackingTexts_Pos.Length; ++i)
            {
                _dicCurHackingTexts.Add(_hackingTexts_Pos[i], new());
            }

            _curHackingBarIndex = 0;
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex]);
            _monitorHackingScreen.alpha = 0f;
            _monitorHackingScreen.interactable = false;
            _monitorHackingScreen.blocksRaycasts = false;
            _monitorHackingScreen.gameObject.SetActive(false);
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _monitorHackingScreen;
            settings = _fadeSettings;
            return true;
        }

        public override void OnCompleteOpen()
        {
            if (_hackingTextPool == null)
            {
                ulong entityID = gameObject.GetEntityULongID();
                PoolManager.Instance.AddPool(entityID, _hackingTextPrefab, _hackingPoolSize, PoolName.MonitorHacking);
                PoolManager.Instance.TryGetPool(entityID, out _hackingTextPool);
            }

            _startHackingCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            StartMonitorHacking(_startHackingCts.Token).Forget();
        }

        public override void OnCompleteClose()
        {
            _startHackingCts?.Cancel();
            _startHackingCts?.Dispose();
            _startHackingCts = null;
        }

        private async UniTaskVoid StartMonitorHacking(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_startHackingDelay), cancellationToken: token);
                MoveHackingTexts(token).Forget();
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < _hackingTexts_Pos.Length; ++i)
                    {
                        TMP_Text hackingText = _hackingTextPool.Pop();
                        hackingText.rectTransform.SetParent(_hackingTexts_Pos[i]);
                        hackingText.rectTransform.anchoredPosition = Vector2.zero;
                        _dicCurHackingTexts[_hackingTexts_Pos[i]].Add(hackingText);
                    }
                    await UniTask.Delay(TimeSpan.FromSeconds(_hackingTextDelay), cancellationToken: token);
                }

            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async UniTaskVoid MoveHackingTexts(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (List<TMP_Text> hackingTexts in _dicCurHackingTexts.Values)
                    {
                        foreach (var hackingText in hackingTexts)
                        {
                            float targetPosX = hackingText.rectTransform.anchoredPosition.x;
                            float targetPosY = hackingText.rectTransform.anchoredPosition.y - (Time.deltaTime * _hackingTextSpeed);
                            hackingText.rectTransform.anchoredPosition = new(targetPosX, targetPosY);
                            if (hackingText.rectTransform.position.y <= _hackingBar.position.y)
                            {
                                hackingTexts.Remove(hackingText);
                                _hackingTextPool.Return(hackingText);
                                break;
                            }
                        }
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPreUpdate, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void MoveHackingBar(bool left)
        {
            if (left)
            {
                if (_curHackingBarIndex == 0)
                {
                    return;
                }
                --_curHackingBarIndex;
            }
            else
            {
                if (_curHackingBarIndex == _hackingSlots.Length - 1)
                {
                    return;
                }
                ++_curHackingBarIndex;
            }
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex]);
            _hackingBar.anchoredPosition = new(0f, _yOffset_HackingBar);
        }

    }
}
