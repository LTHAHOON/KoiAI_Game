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

    public partial class MonitorHackingView : PopUpWindow_Canvas
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
        private float _dangerRatioWhenDanger = 60f;
        [SerializeField]
        private float _dangerRatioWhenSuccess = 45f;
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

        [SerializeField]
        private CanvasGroup _loadingRewardCavasGroup;
        [SerializeField]
        private TMP_Text _loadingRewardProgressText;
        [SerializeField]
        private float _maxLoadingDelay = 0.6f;
        [SerializeField]
        private Button _restartHackingButton;
        [SerializeField]
        private CanvasGroup _restartHackingCanvasGroup;

        private const string TARGET_LOADING_PROGRESS = "................................................";
        private int _curSignal;
        private float _curRemainingTime;
        private RectTransform _screenRectTransform;
        private List<MonitorHackingSlot> _hackingSlots;
        private List<RectTransform> _hackingTexts_Pos;

        private int _curHackingBarIndex;
        private Pool<TMP_Text> _hackingTextPool;
        private CancellationTokenSource _startHackingCts;
        private bool _hackingFailed;
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
            ResetHackingSession();
        }

        public override bool TryGetFadeSettings(out CanvasGroup canvasGroup, out CanvasGroupFadeSettings settings)
        {
            canvasGroup = _monitorHackingScreen;
            settings = _fadeSettings;
            return true;
        }

        public override void OnBeginOpen()
        {
            _parentMonitorScreenView.SetCloseButtonInteractable(true);
            _restartHackingButton.onClick.AddListener(RestartHacking);
            if (_hackingTextPool == null)
            {
                ulong entityID = _hackingTextPrefab.GetEntityULongID();
                PoolManager.Instance.AddPool(entityID, _hackingTextPrefab, _hackingPoolSize, PoolName.MonitorHacking);
                PoolManager.Instance.TryGetPool(entityID, out _hackingTextPool);
            }
        }

        public override void OnCompleteOpen()
        {
            StartHackingSession();
        }

        public override void OnCompleteClose()
        {
            ResetHackingSession();
            _restartHackingButton.onClick.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            ulong entityID = _hackingTextPrefab.GetEntityULongID();
            PoolManager.Instance.RemovePool<TMP_Text>(entityID);
            _hackingTextPool = null;
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
