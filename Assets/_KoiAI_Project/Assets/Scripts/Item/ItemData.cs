using UnityEngine;

namespace KoiAI.Item
{
    using System;
    using KoiAI.Core;
    using KoiAI.UI.HUD;
    using NaughtyAttributes;

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

    public abstract class ItemData : EntityData
    {
        [SerializeField]
        private ItemBase _itemPrefab;

        [SerializeField]
        private Texture2D _itemTex;
        [SerializeField]
        private Mesh _itemMesh;
        [SerializeField]
        private Material[] _itemMaterials;
        [SerializeField]
        private string _itemName;
        [SerializeField]
        private string _itemDescription;
        [SerializeField]
        private bool _isCreatableObj = false;
        
        private Sprite _itemIcon;

        public Sprite ItemIcon
        {
            get
            {
                if (_itemIcon == null && _itemTex != null)
                {
                    _itemIcon = Sprite.Create(
                        _itemTex,
                        new Rect(0f, 0f, _itemTex.width, _itemTex.height),
                        new Vector2(0.5f, 0.5f));
                }
                return _itemIcon;
            }
        }   

        public Mesh ItemMesh => _itemMesh;
        public Material[] ItemMaterials => _itemMaterials;
        public bool IsCreatableObj => _isCreatableObj;
        public ItemBase ItemPrefab => _itemPrefab;
        public Texture2D ItemTex => _itemTex;
        public string ItemName => _itemName;
        public string ItemDescription => _itemDescription;
        public Guid ItemId => GetEntityID();
    }
}