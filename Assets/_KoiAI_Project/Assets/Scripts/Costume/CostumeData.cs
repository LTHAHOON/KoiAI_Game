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

        private VisualElement _costumeSlot;

        public void SetCostumeSlot(VisualElement costumeSlot)
        {
            _costumeSlot = costumeSlot;
        }

        public CostumeCategory CostumeCategory => _costumeCategory;
        public Texture2D CostumeTexture => _costumeTexture; 
        public string CostumeName => _costumeName;
        public GameObject CostumePrefab => _costumePrefab;
        public VisualElement CostumeSlot => _costumeSlot;
    }
}
