using System.Collections;
using System.Collections.Generic;
using KoiAI.Input;
using KoiAI.Item;
using KoiAI.Pool;
using KoiAI.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.UI
{
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
        private Coroutine _returnItemsCoroutine;
        private Transform _poolStorage;
        private CanvasGroup _canvasGroup;
        private int _selectedIndex;
        private int _firstVisibleIndex;
        private bool _isInputSubscribed;
        private bool _isDropTableVisible;

        private void Awake()
        {
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
            HideItems();
            ReturnPendingItems();
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

        public void ShowItems(IReadOnlyList<ItemData> itemDatas)
        {
            HideItems();
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
                _returnItemsCoroutine = StartCoroutine(IEReturnItemsAfterFade());
            }
            else
            {
                _canvasGroup.alpha = 0f;
            }
        }

        private IEnumerator IEReturnItemsAfterFade()
        {
            bool areItemsHidden = false;
            while (!areItemsHidden)
            {
                areItemsHidden = true;
                for (int i = 0; i < _returningItems.Count; i++)
                {
                    if (_canvasGroup.alpha > 0.01f)
                    {
                        areItemsHidden = false;
                        break;
                    }
                }

                if (!areItemsHidden)
                {
                    yield return null;
                }
            }

            _returnItemsCoroutine = null;
            ReturnPendingItems();
        }

        private void ReturnPendingItems()
        {
            if (_returnItemsCoroutine != null)
            {
                StopCoroutine(_returnItemsCoroutine);
                _returnItemsCoroutine = null;
            }

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
            _isInputSubscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (!_isInputSubscribed || InputService.PlayerIA == null)
            {
                return;
            }

            InputService.PlayerIA.Global.ScrollWheel.performed -= OnScrollWheel;
            _isInputSubscribed = false;
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
            if (_selectedIndex < _firstVisibleIndex)
            {
                _firstVisibleIndex = _selectedIndex;
            }
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
