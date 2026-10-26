using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
    using System.Text;
    using KoiAI.Item;

    public class DropTableItem : MonoBehaviour
    {
        private ItemData _itemData;
        public ItemData ItemData => _itemData;
        [SerializeField]
        private Image _dropTableItemImage;
        [SerializeField]
        private TextMeshProUGUI _itemNameText;
        [SerializeField]
        private TextMeshProUGUI _itemCountText;
        [SerializeField]
        private TextMeshProUGUI _itemDescriptionText;
        [SerializeField]
        private Image _itemIconImage;

        private StringBuilder _sb = new();
        public void SetDropTableItem(ItemData itemData)
        {
            if (itemData == null)
            {
                return;
            }

            _itemData = itemData;
            gameObject.SetActive(true);

            _itemNameText.text = itemData.ItemName;
            _sb.Append(itemData.ItemCount);
            _itemCountText.SetText(_sb);

            _itemIconImage.sprite = itemData.ItemIcon;
            _itemDescriptionText.text = itemData.ItemDescription;
            _sb.Clear();
        }

        public void SetSelected(bool isSelected, Color selectedColor, Color normalColor)
        {
            if (_dropTableItemImage == null)
            {
                return;
            }
            _dropTableItemImage.color = isSelected ? selectedColor : normalColor;
        }

        public void SetDropTableItemVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void Clear()
        {
            _itemData = null;
            _itemNameText.text = string.Empty;
            _itemIconImage.sprite = null;
            _itemDescriptionText.text = string.Empty;
        }
    }
}
