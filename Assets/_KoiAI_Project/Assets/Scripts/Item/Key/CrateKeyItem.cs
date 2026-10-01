using UnityEngine;

using KoiAI.Interact;
using KoiAI.Player;

namespace KoiAI.Item
{
    /// <summary>
    /// Runtime inventory item for a crate key.
    /// </summary>
    public class CrateKeyItem : ResourceBase
    {
        [SerializeField]
        private CrateKeyData _crateKeyData;

        public override ItemData GetItemData() => _crateKeyData;

        public CrateKeyType KeyType => _crateKeyData != null
            ? _crateKeyData.KeyType
            : CrateKeyType.None;

        public override void UseItem()
        {
            if (ItemOwner == null ||
                !ItemOwner.TryGetComponent(out PlayerInteractor playerInteractor) ||
                !playerInteractor.TryGetInteractable(out CrateInteractable crate))
            {
                return;
            }

            crate.Interact(new CrateInteractContext
            {
                KeyType = KeyType
            });
        }

        public CrateKeyData GetCrateKeyData() => _crateKeyData;
    }
}
