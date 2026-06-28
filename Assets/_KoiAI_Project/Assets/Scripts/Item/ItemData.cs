using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.UI.HUD;
    
    public enum WeaponType
    {
        Cannon,
        Gun,
        Sword,
    
    }

    public enum ProjectileType
    {
        CannonBall,
        Bullet,
    }

    public class WeaponData : ItemData
    {
        [SerializeField]
        private WeaponType _weaponType;
        [SerializeField]
        private float _weaponDamage;

        public WeaponType WeaponType => _weaponType;
        public float WeaponDamage => _weaponDamage;
    }

    public class ProjectileData : ItemData
    {
        [SerializeField]
        private ProjectileType _projectileType;
        [SerializeField]
        private int _projectileCount;
        [SerializeField]
        private float _projectileDamage;

        public ProjectileType ProjectileType => _projectileType;
        public float Damage => _projectileDamage;
        public int ProjectileCount => _projectileCount;
    }

    public abstract class ItemData : ScriptableObject
    {
        [SerializeField]
        private ItemBase _itemPrefab;
        [SerializeField]
        private Texture2D _itemTex;
        [SerializeField]
        private Mesh _itemMesh;
        [SerializeField]
        private Material _itemMaterial;
        [SerializeField]
        private string _itemName;
        [SerializeField]
        private ulong _itemId;
        [SerializeField]
        private bool _isCreatableObj = false;

        public Mesh ItemMesh => _itemMesh;
        public Material ItemMaterial => _itemMaterial;
        public bool IsCreatableObj => _isCreatableObj;
        public ItemBase ItemPrefab => _itemPrefab;
        public Texture2D ItemTex => _itemTex;
        public string ItemName => _itemName;
        public ulong ItemId => _itemId;
    }
}