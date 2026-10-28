using KoiAI.Interact;
using UnityEngine;

namespace KoiAI.Item
{
    /// <summary>
    /// 상자키 데이터 저장용
    /// </summary>
    [CreateAssetMenu(fileName = "new CrateKeyData", menuName = "KoiAI/Item/CrateKeyData")]
    public class CrateKeyData : ItemData
    {
        [SerializeField]
        private CrateKeyType _keyType;

        public CrateKeyType KeyType => _keyType;
    }
}
