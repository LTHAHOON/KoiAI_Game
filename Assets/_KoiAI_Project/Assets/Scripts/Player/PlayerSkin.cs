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
        [SerializeField]
        private Transform _feetPoint;

        public Transform ResoucePoint => _resoucePoint;
        public Transform WeaponPoint => _weaponPoint;
        public Transform FeetPoint => _feetPoint;
    }
}
