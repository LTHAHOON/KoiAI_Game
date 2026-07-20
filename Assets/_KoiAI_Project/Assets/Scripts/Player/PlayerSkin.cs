using UnityEngine;

namespace KoiAI.Player
{
    using KoiAI.Skin;
    
    public class PlayerSkin : Skin
    {
        [SerializeField]
        private Material _faceMaterial;
        [SerializeField]
        private Material _bodyMaterial;
        [SerializeField] 
        private Transform _resoucePoint;
        [SerializeField] 
        private Transform _weaponPoint;
        [SerializeField]
        private Transform _feetPoint;
        [SerializeField]
        private Transform _capPoint;
        [SerializeField]
        private Transform _glassesPoint;
        [SerializeField]
        private Transform _capePoint;

        public Material FaceMaterial => _faceMaterial;
        public Material BodyMaterial => _bodyMaterial;  
        public Transform ResoucePoint => _resoucePoint;
        public Transform WeaponPoint => _weaponPoint;
        public Transform FeetPoint => _feetPoint;
        public Transform CapPoint => _capPoint;
        public Transform GlassesPoint => _glassesPoint;
        public Transform CapePoint => _capePoint;
    }
}
