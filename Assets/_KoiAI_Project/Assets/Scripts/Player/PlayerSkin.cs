using KoiAI.Item;
using UnityEngine;

namespace KoiAI.Player
{
    using KoiAI.Skin;
    
    public class PlayerSkin : Skin
    {
        [SerializeField] 
        private Transform _resoucePoint;
        [SerializeField] 
        private Transform _weaponPoint;

        public Transform ResoucePoint => _resoucePoint;
        public Transform WeaponPoint => _weaponPoint;
    }
}
