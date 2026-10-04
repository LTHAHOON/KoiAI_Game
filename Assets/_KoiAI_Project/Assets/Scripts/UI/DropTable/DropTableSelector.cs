using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

namespace KoiAI.UI
{
    using KoiAI.Input;
    using KoiAI.Item;
    using KoiAI.Player;
    using KoiAI.Pool;
    using KoiAI.Utilities;

    [RequireComponent(typeof(CanvasGroup))]
    public class DropTableSelector : MonoBehaviour
    {
        [Header("DropTable 설정")]
        [SerializeField]
        private RectTransform _itemContainer;
        [SerializeField]
        private DropTableItem _dropTableItemPrefab;
        [SerializeField]
        private PoolSize _dropTableItemPoolSize = new(3, 20);
        [SerializeField]
        private Color _normalColor = new(0.5f, 0.5f, 0.5f, 0.235f);
        [SerializeField]
        private Color _selectedColor = new(0.35f, 0.65f, 1f, 0.8f);
        [SerializeField]
        [Min(1)]
        private int _scrollStep = 1;
        [SerializeField]
        [Min(1)]
        private int _visibleItemCount = 3;

        private readonly List<DropTableItem> _dropTableItems = new();
        private readonly List<DropTableItem> _returningItems = new();
        private Pool<DropTableItem> _dropTableItemPool;
        private CancellationTokenSource _returnItemsCancellation;
        private Transform _poolStorage;
        private CanvasGroup _canvasGroup;
        private int _selectedIndex;
        private int _firstVisibleIndex;
        private bool _isInputSubscribed;
        private bool _isDropTableVisible;
        private PlayerEquipment _playerEquipment;
        private Action<ItemData> _onItemTaken;

        private void Awake()
        {
            _playerEquipment = FindAnyObjectByType<PlayerEquipment>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            // 씬에 배치된 행은 풀의 원본으로만 사용합니다.
            if (_itemContainer != null)
            {
                for (int i = 0; i < _itemContainer.childCount; i++)
                {
                    DropTableItem item = _itemContainer.GetChild(i).GetComponent<DropTableItem>();
                    if (item != null)
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Start()
        {
            InitializePool();
            SubscribeInput();
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();

            _returnItemsCancellation?.Cancel();
            _returnItemsCancellation?.Dispose();
            _returnItemsCancellation = null;

            _returningItems.AddRange(_dropTableItems);
            _dropTableItems.Clear();
            _selectedIndex = 0;
            _firstVisibleIndex = 0;
            SetDropTableVisible(false);
            _canvasGroup.alpha = 0f;

            if (_returningItems.Count > 0 && PoolManager.Instance != null && _dropTableItemPool != null)
            {
                List<DropTableItem> items = new(_returningItems);
                _returningItems.Clear();
                PoolManager.Instance.StartCoroutine(IEReturnItemsAfterDisable(items));
            }
        }

        private IEnumerator IEReturnItemsAfterDisable(List<DropTableItem> items)
        {
            yield return null;

            for (int i = 0; i < items.Count; i++)
            {
                DropTableItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                item.Clear();
                item.transform.SetParent(_poolStorage, false);
                _dropTableItemPool.Return(item);
            }
        }

        private void LateUpdate()
        {
            float targetAlpha = _isDropTableVisible ? 1f : 0f;
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, Time.deltaTime * 10f);
        }

        private void InitializePool()
        {
            if (_dropTableItemPool != null || _dropTableItemPrefab == null || PoolManager.Instance == null)
            {
                return;
            }

            ulong prefabId = _dropTableItemPrefab.GetEntityULongID();
            PoolManager.Instance.AddPool(prefabId, _dropTableItemPrefab,
                _dropTableItemPoolSize, PoolName.DropTable);
            PoolManager.Instance.TryGetPool(prefabId, out _dropTableItemPool);
            _poolStorage = PoolStorage.GetStorage(PoolName.DropTable);
        }

        public void ShowItems(IReadOnlyList<ItemData> itemDatas, Action<ItemData> onItemTaken = null)
        {
            HideItems();
            _onItemTaken = onItemTaken;
            InitializePool();
            if (_dropTableItemPool == null || _itemContainer == null || itemDatas == null)
            {
                return;
            }

            for (int i = 0; i < itemDatas.Count; i++)
            {
                if (itemDatas[i] == null)
                {
                    continue;
                }

                DropTableItem item = _dropTableItemPool.Pop();
                item.transform.SetParent(_itemContainer, false);
                item.SetDropTableItem(itemDatas[i]);
                _dropTableItems.Add(item);
            }

            _selectedIndex = 0;
            _firstVisibleIndex = 0;
            UpdateVisibleItems();
            UpdateSelectedColors();
            SetDropTableVisible(_dropTableItems.Count > 0);
        }

        public void HideItems()
        {
            _onItemTaken = null;
            ReturnPendingItems();
            SetDropTableVisible(false);
            if (_dropTableItemPool == null)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            for (int i = 0; i < _dropTableItems.Count; i++)
            {
                DropTableItem item = _dropTableItems[i];
                item.SetDropTableItemVisible(true);
                _returningItems.Add(item);
            }

            _dropTableItems.Clear();
            _selectedIndex = 0;
            _firstVisibleIndex = 0;

            if (_returningItems.Count > 0)
            {
                _returnItemsCancellation?.Cancel();
                _returnItemsCancellation?.Dispose();
                _returnItemsCancellation = new CancellationTokenSource();
                ReturnItemsAfterFadeAsync(_returnItemsCancellation.Token).Forget();
            }
            else
            {
                _canvasGroup.alpha = 0f;
            }
        }

        private async UniTask ReturnItemsAfterFadeAsync(CancellationToken cancellationToken)
        {
            while (_canvasGroup.alpha > 0.01f)
            {
                await UniTask.NextFrame(cancellationToken);
            }

            _returnItemsCancellation = null;
            ReturnPendingItems();
        }

        private void ReturnPendingItems()
        {
            for (int i = 0; i < _returningItems.Count; i++)
            {
                DropTableItem item = _returningItems[i];
                item.Clear();
                item.transform.SetParent(_poolStorage, false);
                _dropTableItemPool.Return(item);
            }

            _returningItems.Clear();
            if (!_isDropTableVisible)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        public void SetDropTableVisible(bool isVisible)
        {
            _isDropTableVisible = isVisible;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = isVisible;
            }
        }

        private void SubscribeInput()
        {
            if (_isInputSubscribed || InputService.PlayerIA == null)
            {
                return;
            }

            InputService.PlayerIA.Global.ScrollWheel.performed += OnScrollWheel;
            InputService.PlayerIA.Player.GetItem.performed += OnGetItem;
            _isInputSubscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!_isInputSubscribed || InputService.PlayerIA == null)
            {
                return;
            }

            InputService.PlayerIA.Global.ScrollWheel.performed -= OnScrollWheel;
            InputService.PlayerIA.Player.GetItem.performed -= OnGetItem;
            _isInputSubscribed = false;
        }

        private void OnGetItem(InputAction.CallbackContext context)
        {
            if (!context.performed || !_isDropTableVisible || _dropTableItems.Count == 0 || _playerEquipment == null)
            {
                return;
            }

            DropTableItem selectedItem = _dropTableItems[_selectedIndex];
            if (selectedItem == null || !_playerEquipment.PickUpItem(selectedItem.ItemData))
            {
                return;
            }

            _onItemTaken?.Invoke(selectedItem.ItemData);

            _dropTableItems.RemoveAt(_selectedIndex);
            _returningItems.Add(selectedItem);
            selectedItem.SetDropTableItemVisible(false);
            ReturnPendingItems();
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _dropTableItems.Count - 1);
            if (_dropTableItems.Count == 0)
            {
                HideItems();
                return;
            }

            UpdateVisibleItems();
            UpdateSelectedColors();
        }

        private void OnScrollWheel(InputAction.CallbackContext context)
        {
            if (!_isDropTableVisible || _dropTableItems.Count == 0)
            {
                return;
            }

            float scrollY = context.ReadValue<Vector2>().y;
            if (Mathf.Approximately(scrollY, 0f))
            {
                return;
            }

            int direction = scrollY > 0f ? -1 : 1;
            SelectItem(_selectedIndex + direction * _scrollStep);
        }

        private void SelectItem(int index)
        {
            int nextIndex = Mathf.Clamp(index, 0, _dropTableItems.Count - 1);
            if (nextIndex == _selectedIndex)
            {
                return;
            }

            _dropTableItems[_selectedIndex].SetSelected(false, _selectedColor, _normalColor);
            _selectedIndex = nextIndex;
            _dropTableItems[_selectedIndex].SetSelected(true, _selectedColor, _normalColor);

            int visibleCount = Mathf.Max(1, _visibleItemCount);
            //위쪽으로 벗어난 경우(_selectedIndex가 화면상에 보일수있는 젤 위쪽이 되게 합니다.)
            if (_selectedIndex < _firstVisibleIndex)
            {
                _firstVisibleIndex = _selectedIndex;
            }
            //아래쪽으로 벗어난 경우(_selectedIndex가 화면상에 보일수있는 젤 아래쪽이 되게 합니다.)
            else if (_selectedIndex >= _firstVisibleIndex + visibleCount)
            {
                _firstVisibleIndex = _selectedIndex - visibleCount + 1;
            }

            UpdateVisibleItems();
        }

        private void UpdateVisibleItems()
        {
            int lastVisibleIndex = _firstVisibleIndex + Mathf.Max(1, _visibleItemCount);
            for (int i = 0; i < _dropTableItems.Count; i++)
            {
                bool isVisible = i >= _firstVisibleIndex && i < lastVisibleIndex;
                _dropTableItems[i].SetDropTableItemVisible(isVisible);
            }
        }

        private void UpdateSelectedColors()
        {
            for (int i = 0; i < _dropTableItems.Count; i++)
            {
                _dropTableItems[i].SetSelected(i == _selectedIndex, _selectedColor, _normalColor);
            }
        }
    }
}
