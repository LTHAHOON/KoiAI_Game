using JetBrains.Annotations;
using UnityEngine;

namespace KoiAI.Utilities
{
    public static class EntityUtils
    {
        /// <summary>
        /// Entity ID 가져오기 
        /// </summary>
        public static ulong GetEntityULongID([CanBeNull] this Object obj)
        {
            if (!obj)
            {
                throw new System.ArgumentNullException(nameof(obj));
            }
            EntityId entityId = obj.GetEntityId();
            ulong id = EntityId.ToULong(entityId);
            return id;
        }
    }
}
