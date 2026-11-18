using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace KoiAI.Item
{
    [Serializable]
    public class ItemPropDropper
    {
        [Header("드랍 아이템 프리팹 리스트")]
        [SerializeField]
        private List<ItemProp> _dropItemPropList;
        [SerializeField]
        private bool _useRandomItemProp = false;
        [AllowNesting]
        [EnableIf(nameof(_useRandomItemProp))]
        [SerializeField, Min(1)]
        private int _minDropItemPropCount = 1;
        [SerializeField]
        private float _dropRadius = 5f;
        [SerializeField]
        private float _dropJumpPower = 3f;
        [SerializeField]
        private float _dropJumpDuration = 0.7f;
        [SerializeField]
        private bool _useAutoDropOrgin = true;
        [AllowNesting]
        [DisableIf(nameof(_useAutoDropOrgin))]
        [SerializeField]
        private Vector3 _dropOrigin = Vector3.zero;
        [SerializeField]
        private bool _useDropOriginOffset = true;
        [AllowNesting]
        [EnableIf(nameof(_useDropOriginOffset))]
        [SerializeField]
        private Vector3 _dropOriginOffset = new(0, 1, 0);
        
        public void DropItemProps(Vector3 origin)
        {
            if(_dropItemPropList == null || _dropItemPropList.Count <= 0)
            {
                return;
            }
            if(!_useAutoDropOrgin)
            {
                origin = _dropOrigin;
            }
            if(_useDropOriginOffset)
            {
                origin += _dropOriginOffset;
            }
            
            int maxTargetIndex = _dropItemPropList.Count - 1;
            if(_useRandomItemProp)
            {
                if(_minDropItemPropCount > _dropItemPropList.Count)
                {
                    Debug.LogError("Error: _minDropItemPropCount은 _dropItemPropList.Count보다 클 수 없습니다.");
                    return;
                }
                maxTargetIndex = UnityEngine.Random.Range(_minDropItemPropCount - 1, _dropItemPropList.Count - 1);
            }

            for(int i = 0; i <= maxTargetIndex; ++i)
            {
                ItemProp newDropItemProp = UnityEngine.Object.Instantiate(_dropItemPropList[i], origin, Quaternion.identity);
                if(!newDropItemProp)
                {
                    continue;
                }
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * _dropRadius;
                randomOffset.y = 0f;
                Vector3 targetPos = origin + randomOffset;
                newDropItemProp.transform.DOJump(targetPos, _dropJumpPower, 1, _dropJumpDuration);
            }

        }
    }
}
