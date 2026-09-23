using System;
using UnityEngine;
using NaughtyAttributes;

namespace KoiAI.Core
{
    public enum EntityType
    {
        PlayerCharacter,
        EnemyCharacter,
        Costume,
        Item
    }

    public abstract class EntityData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private EntityType _entityType;
        [ReadOnly]
        [SerializeField] 
        private string _guidString;
        
        private Guid _guid;


        public void OnBeforeSerialize()
        {
            if (_guid == Guid.Empty || string.IsNullOrEmpty(_guidString))
            {
                _guid = Guid.NewGuid();
                _guidString = _guid.ToString();
            }
        }

        public void OnAfterDeserialize()
        {
            Guid.TryParse(_guidString, out _guid);
        }

        public EntityType EntityType => _entityType;
        public Guid GetEntityID() => _guid;
        
    }
}
