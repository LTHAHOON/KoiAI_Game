using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.UI
{
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
        private TextMeshProUGUI _itemDescriptionText;
        [SerializeField]
        private Image _itemIconImage;

        public void SetDropTableItem(ItemData itemData)
        {
            if (itemData == null)
            {
                return;
            }

            _itemData = itemData;
            gameObject.SetActive(true);

            if (_itemNameText != null)
            {
                _itemNameText.text = itemData.ItemName;
            }

            if (_itemIconImage != null)
            {
                _itemIconImage.sprite = itemData.ItemIcon;
            }

            if (_itemDescriptionText != null)
            {
                _itemDescriptionText.text = itemData.ItemDescription;
            }
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
            if (_itemNameText != null)
            {
                _itemNameText.text = string.Empty;
            }

            if (_itemIconImage != null)
            {
                _itemIconImage.sprite = null;
            }

            if (_itemDescriptionText != null)
            {
                _itemDescriptionText.text = string.Empty;
            }
        }
    }
}

