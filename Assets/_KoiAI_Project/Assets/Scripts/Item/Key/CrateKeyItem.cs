using UnityEngine;

using KoiAI.Interact;
using KoiAI.Player;

namespace KoiAI.Item
{
    /// <summary>
    /// 런타임용 상자 키 아이템
    /// </summary>
    public class CrateKeyItem : ResourceBase
    {
        private CrateKeyData _crateKeyData;

        public override ItemData GetItemData() => InitializedItemData ?? _crateKeyData;

        public CrateKeyType KeyType => _crateKeyData != null
            ? _crateKeyData.KeyType
            : CrateKeyType.None;

        public override void UseItem()
        {
            if (ItemOwner == null || !ItemOwner.TryGetComponent(out PlayerInteractor playerInteractor))
            {
                return;
            }

            if (playerInteractor.TryGetInteractable(out CrateInteractable crate))
            {
                crate.Interact(new CrateInteractContext
                {
                    KeyType = KeyType
                });

            }


        }
    }
}
