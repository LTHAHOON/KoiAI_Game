using KoiAI.Interact;
using UnityEngine;

namespace KoiAI.Item
{
    /// <summary>
    /// Data asset for a key that can open a CrateInteractable.
    /// </summary>
    [CreateAssetMenu(fileName = "new CrateKeyData", menuName = "KoiAI/Item/CrateKeyData")]
    public class CrateKeyData : ItemData
    {
        [SerializeField]
        private CrateKeyType _keyType;

        public CrateKeyType KeyType => _keyType;
    }
}
