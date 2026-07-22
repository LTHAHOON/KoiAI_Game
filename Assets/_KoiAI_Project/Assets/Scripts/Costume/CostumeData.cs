using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KoiAI.Costume
{
    [CreateAssetMenu(fileName = "new CostumeData", menuName = "KoiAI/Costume/CostumeData")]
    public class CostumeData : ScriptableObject
    {
        [SerializeField]
        private CostumeCategory _costumeCategory;
        [SerializeField]
        private Texture2D _costumeTexture;
        [SerializeField]
        private string _costumeName;
        [SerializeField]
        private GameObject _costumePrefab;
        [Header("중복 착용 여부")]
        [SerializeField]
        private bool _canMultipleCostume = false;
        [HideInInspector]
        [SerializeField]
        private Guid _guid = Guid.NewGuid();
        
        private VisualElement _costumeSlot;

        public void SetCostumeSlot(VisualElement costumeSlot)
        {
            _costumeSlot = costumeSlot;
        }
        public Guid GetGUID() => _guid;

        public CostumeCategory CostumeCategory => _costumeCategory;
        public Texture2D CostumeTexture => _costumeTexture; 
        public string CostumeName => _costumeName;
        public GameObject CostumePrefab => _costumePrefab;
        public bool CanMulitpleCostume => _canMultipleCostume; 
        public VisualElement CostumeSlot => _costumeSlot;
    }
}
