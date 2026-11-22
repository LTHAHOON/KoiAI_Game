using TMPro;
using UnityEngine;

namespace KoiAI.UI
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Text;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using KoiAI.Pool;
    using KoiAI.Utilities;
    using Unity.VisualScripting;
    using UnityEngine.UI;

    public class MonitorHackingView : PopUpWindow_Canvas
    {
        [SerializeField]
        private RectTransform _hackingSlotParent;
        [SerializeField]
        private MonitorHackingSlot _hackingSlotPrefab;
        [SerializeField]
        private float _hackingSlotSuffleDelay = 0.1f;
        [SerializeField]
        private string _completedWord;
        [SerializeField]
        private int _minCodeNumber = 1000;
        [SerializeField]
        private int _maxCodeNumber = 9999;
        [SerializeField]
        private Material _dangerMaterialPreset;
        [SerializeField]
        private Material _successMaterialPreset;
        [SerializeField]
        private float _dangerRatio = 70f;
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
        private RectTransform _hackingText_PosPrefab;
        [SerializeField]
        private RectTransform _hackingText_PosParent;
        [SerializeField]
        private RectTransform _removeHackingTextTransform;

        [SerializeField]
        private float _timeLimit = 30f;
        [SerializeField]
        private TMP_Text _timeText;
        [SerializeField]
        private Image _timerGuageImage; 
        [SerializeField]
        private RectTransform _timerCompass;
        [SerializeField]
        private List<Image> _signalImages = new();
        [SerializeField, Min(0f)]
        private float _signalDrainDuration = 0.3f;
        
        private int _curSignal;
        private readonly List<Tween> _signalTweens = new();
        private float _curRemainingTime;
        private RectTransform _screenRectTransform;
        private List<MonitorHackingSlot> _hackingSlots;
        private List<RectTransform> _hackingTexts_Pos;

        private int _curHackingBarIndex;
        private Pool<TMP_Text> _hackingTextPool;
        private CancellationTokenSource _startHackingCts;
        private Dictionary<RectTransform, List<TMP_Text>> _dicCurHackingTexts;
        private int[] _hackingTextPositionOrder;
        private readonly Vector3[] _hackingTextWorldCorners = new Vector3[4];
        private readonly Vector3[] _hackingBarWorldCorners = new Vector3[4];
        private MonitorScreenView _parentMonitorScreenView;
        private int _completedCount = 0;
        public Action<bool> OnReachHackingTextWithBar;

        public override void InitializePopUp(PopUpWindow_Canvas parentWindow = null)
        {
            if(parentWindow is not MonitorScreenView monitorScreenView)
            {
                return;
            }
            _parentMonitorScreenView = monitorScreenView;
            _screenRectTransform = _monitorHackingScreen.transform as RectTransform;
            _hackingSlots = new();
            _hackingTexts_Pos = new();
            for(int i = 0; i < _completedWord.Length; ++i)
            {
                MonitorHackingSlot monitorHackingSlot = Instantiate(_hackingSlotPrefab, _hackingSlotParent);
                monitorHackingSlot.Initalize(_completedWord[i], _hackingSlotSuffleDelay);
                _hackingSlots.Add(monitorHackingSlot);

                RectTransform _hackingText_PosTransform = Instantiate(_hackingText_PosPrefab, _hackingText_PosParent);
                _hackingTexts_Pos.Add(_hackingText_PosTransform);
            }

            _dicCurHackingTexts = new();
            _hackingTextPositionOrder = new int[_hackingTexts_Pos.Count];
            for (int i = 0; i < _hackingTexts_Pos.Count; ++i)
            {
                _dicCurHackingTexts.Add(_hackingTexts_Pos[i], new());
                _hackingTextPositionOrder[i] = i;
            }

            _monitorHackingScreen.alpha = 0f;
            _monitorHackingScreen.interactable = false;
            _monitorHackingScreen.blocksRaycasts = false;
            _monitorHackingScreen.gameObject.SetActive(false);
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            _curHackingBarIndex = 0;
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex].transform);
            _timeText.SetTextFormat("{0:F2}", _timeLimit);
            ResetSignals();
            _hackingBar.anchoredPosition = new(0f, _yOffset_HackingBar);
            canvasGroup = _monitorHackingScreen;
            settings = _fadeSettings;
            return true;
        }

        public override void OnCompleteOpen()
        {
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
            _completedCount = 0;
            _startHackingCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            StartMonitorHacking(_startHackingCts.Token).Forget();
        }

        public override void OnCompleteClose()
        {
            _startHackingCts?.Cancel();
            _startHackingCts?.Dispose();
            _startHackingCts = null;
            foreach(MonitorHackingSlot hackingSlot in _hackingSlots)
            {
                hackingSlot.EndShuffleTextLoop();
            }
            ClearHackingTexts();
        }

        private void OnDestroy()
        {
            ulong entityID = _hackingTextPrefab.GetEntityULongID();
            PoolManager.Instance.RemovePool<TMP_Text>(entityID);
            _hackingTextPool = null;
        }

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
                signalImage.DOFillAmount(0f, _signalDrainDuration).SetEase(Ease.OutQuad).SetAutoKill();
            }
        }
        
        private async UniTaskVoid StartMonitorHacking(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_startHackingDelay), cancellationToken: token);
                MoveHackingTexts(token).Forget();
                StartTimeLimit(token).Forget();
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    ShuffleHackingTextPositions();
                    for (int i = 0; i < _hackingTexts_Pos.Count; ++i)
                    {
                        SpawnHackingText(_hackingTextPositionOrder[i]);
                        if (i < _hackingTexts_Pos.Count - 1)
                        {
                            float minDelay = Mathf.Max(0f, _minHackingTextDelay);
                            float maxDelay = Mathf.Max(minDelay, _maxHackingTextDelay);
                            float delay = UnityEngine.Random.Range(minDelay, maxDelay);
                            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                        }
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

        private async UniTaskVoid StartTimeLimit(CancellationToken token)
        {
            try
            {
                _curRemainingTime = _timeLimit;
                while(_curRemainingTime > 0)
                {
                    token.ThrowIfCancellationRequested();
                    _curRemainingTime -= Time.deltaTime;
                    _timeText.SetTextFormat("{0:F2}", _curRemainingTime);
                    _timerGuageImage.fillAmount = Mathf.Clamp01(_curRemainingTime / _timeLimit);
                    _timerCompass.localEulerAngles = new(0f, 0f, -(360f - (_curRemainingTime * (360f / _timeLimit))));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ShuffleHackingTextPositions()
        {
            for (int i = _hackingTextPositionOrder.Length - 1; i > 0; --i)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (_hackingTextPositionOrder[i], _hackingTextPositionOrder[randomIndex]) = (_hackingTextPositionOrder[randomIndex], _hackingTextPositionOrder[i]);
            }
        }

        private void SpawnHackingText(int positionIndex)
        {
            RectTransform textPosition = _hackingTexts_Pos[positionIndex];
            TMP_Text hackingText = _hackingTextPool.Pop();
            int codeNumber = UnityEngine.Random.Range(_minCodeNumber, _maxCodeNumber + 1);
            hackingText.SetTextFormat("0x{0}", codeNumber);
            float ratio = UnityEngine.Random.Range(0f, 100f);
            if(ratio <= _dangerRatio)
            {
                hackingText.fontSharedMaterial = _dangerMaterialPreset;
            }
            else
            {
                hackingText.fontSharedMaterial = _successMaterialPreset;
            }
            hackingText.rectTransform.SetParent(textPosition, false);
            hackingText.rectTransform.anchoredPosition = Vector2.zero;
            _dicCurHackingTexts[textPosition].Add(hackingText);
        }

        private void ClearHackingTexts()
        {
            if (_dicCurHackingTexts == null || _hackingTextPool == null)
            {
                return;
            }

            foreach (List<TMP_Text> hackingTexts in _dicCurHackingTexts.Values)
            {
                foreach (TMP_Text hackingText in hackingTexts)
                {
                    _hackingTextPool.Return(hackingText);
                }
                hackingTexts.Clear();
            }
        }

        private async UniTaskVoid MoveHackingTexts(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    Rect hackingBarBounds = GetLocalBounds(_hackingBar, _hackingBarWorldCorners);
                    float removeHackingTextY = _screenRectTransform.InverseTransformPoint(_removeHackingTextTransform.position).y;
                    foreach (List<TMP_Text> hackingTexts in _dicCurHackingTexts.Values)
                    {
                        foreach (TMP_Text hackingText in hackingTexts)
                        {
                            float targetPosX = hackingText.rectTransform.anchoredPosition.x;
                            float targetPosY = hackingText.rectTransform.anchoredPosition.y - (Time.deltaTime * _hackingTextSpeed);
                            hackingText.rectTransform.anchoredPosition = new(targetPosX, targetPosY);
                            Rect hackingTextBounds = GetLocalBounds(hackingText.rectTransform, _hackingTextWorldCorners);
                            if (hackingBarBounds.Overlaps(hackingTextBounds))
                            {
                                bool isSuccess = hackingText.fontSharedMaterial == _successMaterialPreset;
                                if (hackingText.fontSharedMaterial == _dangerMaterialPreset)
                                {
                                    ConsumeSignal();
                                }
                                if(isSuccess && !_hackingSlots[_curHackingBarIndex].IsCompletedText)
                                {
                                    ++_completedCount;
                                    _hackingSlots[_curHackingBarIndex].CompleteText();
                                }
                                else if(!isSuccess  && _hackingSlots[_curHackingBarIndex].IsCompletedText)
                                {
                                    --_completedCount;
                                    _hackingSlots[_curHackingBarIndex].RestartSuffleTextLoop();
                                }
                                OnReachHackingTextWithBar?.Invoke(isSuccess);
                                hackingTexts.Remove(hackingText);
                                _hackingTextPool.Return(hackingText);

                                if (_completedCount == _completedWord.Length)
                                {
                                    await UniTask.Delay(TimeSpan.FromSeconds(2f));
                                    _parentMonitorScreenView.OpenChildView<MonitorHackingRewardView>();
                                    return;
                                }
                                break;
                            }

                            if (hackingTextBounds.center.y <= removeHackingTextY)
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

        private Rect GetLocalBounds(RectTransform target, Vector3[] worldCorners)
        {
            target.GetWorldCorners(worldCorners);
            Vector3 firstCorner = _screenRectTransform.InverseTransformPoint(worldCorners[0]);
            float minX = firstCorner.x;
            float maxX = firstCorner.x;
            float minY = firstCorner.y;
            float maxY = firstCorner.y;

            for (int i = 1; i < worldCorners.Length; ++i)
            {
                Vector3 localCorner = _screenRectTransform.InverseTransformPoint(worldCorners[i]);
                minX = Mathf.Min(minX, localCorner.x);
                maxX = Mathf.Max(maxX, localCorner.x);
                minY = Mathf.Min(minY, localCorner.y);
                maxY = Mathf.Max(maxY, localCorner.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
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
                if (_curHackingBarIndex == _hackingSlots.Count - 1)
                {
                    return;
                }
                ++_curHackingBarIndex;
            }
            _hackingBar.SetParent(_hackingSlots[_curHackingBarIndex].transform);
            _hackingBar.anchoredPosition = new(0f, _yOffset_HackingBar);
        }

        public Vector3 HackingBarWorldPos => _hackingBar.position;
    }
}
