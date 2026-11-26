using UnityEngine;

namespace KoiAI.Item
{
    //종이 데이터 저장용
    [CreateAssetMenu(fileName = "new PaperData", menuName = "KoiAI/Item/PaperData")]
    public class PaperData : ItemData
    {
        [SerializeField]
        private Sprite _paperSprite;

        public Sprite PaperSprite => _paperSprite;
    }
}
