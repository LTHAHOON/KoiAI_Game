using System;
using KoiAI.Core;
using UnityEngine;

namespace KoiAI.Core
{
    public class EntityIdentity : MonoBehaviour
    {
        [SerializeField]
        private EntityData _entityData;

        public void SetEntityData(EntityData entityData)
        {
            _entityData = entityData;
        }
        
        public EntityData EntityData => _entityData;
        public EntityType EntityType => _entityData.EntityType;
        public Guid EntityID => _entityData.GetEntityID();
    }
}
