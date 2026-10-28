using UnityEngine;

namespace KoiAI.Item
{
    //런타임 용 종이 아이템
    public class PaperItem : ResourceBase
    {
        [SerializeField]
        private PaperData _paperData;

        public override ItemData GetItemData() => InitializedItemData ?? _paperData;

        public override void UseItem()
        {
            Debug.Log(InitializedItemData.ItemName);
            if(ItemOwner == null)
            {
                return;
            }


        }
    }
}
