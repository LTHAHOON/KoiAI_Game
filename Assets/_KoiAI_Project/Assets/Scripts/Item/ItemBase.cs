using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Player;
    using static KoiAI.Player.PlayerFeature;

    public abstract class ItemBase: MonoBehaviour
    {
        private ItemData _itemData;
        private PlayerController _itemOwner;
        private Renderer _itemUI;
        private ItemSlotType _currentSlotType;
        public abstract ItemCategory Category { get; }

        public virtual void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            _itemUI = itemUI;
            SetCurrentSlotType(curSlotType);
            SetItemOwner(itemOwner);
        }

        public virtual void SetItemData(ItemData itemData)
        {
            _itemData = itemData;
        }

        public virtual void SetItemCountInSlot() { }

        private void SetItemOwner(PlayerController itemOwner)
        {
            _itemOwner = itemOwner;
        }


        public void SetCurrentSlotType(ItemSlotType curSlotType)
        {
            _currentSlotType = curSlotType;
        }
        public void SetItemActive(bool active)
        {
            enabled = active;
            gameObject.SetActive(active);
        }
        public bool TryGetItemChildClass<T>(out T item) where T : ItemBase
        {
            if(this is T itemClass)
            {
                item = itemClass;
                return true;
            }
            item = null;
            return false;
        }
        public abstract void UseItem();
        public abstract ItemData GetItemData();

        protected ItemData InitializedItemData => _itemData;
        public Renderer GetItemUI() => _itemUI;
        public ItemSlotType GetCurrentSlotType() => _currentSlotType;
        public PlayerController ItemOwner => _itemOwner;
    }
    public abstract class WeaponBase : ItemBase
    {
        private WeaponControllerBase _reloadController;
        private bool _isReloadAnimationPlaying;

        public override void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
        {
            base.Init(itemOwner, itemUI, curSlotType);
            if (_reloadController)
            {
                return;
            }
            _reloadController = GetComponent<WeaponControllerBase>();
            if (_reloadController)
            {
                _reloadController.OnReloadStateChanged += OnReloadStateChanged;
                destroyCancellationToken.Register(DisconnectReloadAnimation);
            }
        }

        private void OnReloadStateChanged(bool isReloading)
        {
            if (!ItemOwner || (isReloading && (!isActiveAndEnabled || GetCurrentSlotType() != ItemSlotType.Equipped)))
            {
                return;
            }
            if (!isReloading && !_isReloadAnimationPlaying)
            {
                return;
            }
            var animParamData = ItemOwner.PlayerAnimatorData.AnimParamData;
            if (animParamData)
            {
                animParamData.SetReloadParameter(ItemOwner.PlayerAnimator, isReloading);
            }
            _isReloadAnimationPlaying = isReloading;
        }

        private void DisconnectReloadAnimation()
        {
            OnReloadStateChanged(false);
            if (_reloadController)
            {
                _reloadController.OnReloadStateChanged -= OnReloadStateChanged;
            }
        }

        protected abstract void ConnectPlayerIA();
        protected abstract void DisConnectPlayerIA();

        public override ItemCategory Category => ItemCategory.Weapon;
    }
    public abstract class ResourceBase : ItemBase
    {
        public override void SetItemCountInSlot()
        {
            if (!ItemOwner)
            {
                return;
            }

            PlayerFeature feature = ItemOwner.GetPlayerFeatureWithProperty(PlayerFeatureProperty.Equipment);
            if(feature is not PlayerEquipment equipment)
            {
                Debug.LogError("ERROR: PlayerFeautre is not playerEquipment");
                return;
            }
            ItemData itemData = GetItemData();
            if (equipment && itemData)
            {
                equipment.SetItemCount(this, itemData.ItemCount);
            }
        }

        public override ItemCategory Category => ItemCategory.Resource;
    }
}
