using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;
using static KoiAI.Player.PlayerFeature;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.Player
{
    public class PlayerAutoFeatureGenerator : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _playerController;

        private PlayerData _playerData;
        private readonly Dictionary<PlayerFeatureProperty, PlayerFeature> _dicPlayerFeature = new();

#if UNITY_EDITOR
        [Button("(Re)Generate Feature Component")]
        public void GeneratePlayerFeature()
        {
            if (_playerController == null)
            {
                Debug.Log("Failed Generate: PlayerController is null");
                return;
            }
            _playerData = _playerController.PlayerData;

            ClearPlayerFeature();
            AddPlayerFeature();

            Debug.Log("Completed Generate Player Feature");
        }
#endif

        private void AddPlayerFeature()
        {
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            if (playerFeatureData)
            {
                PlayerFeatureProperty[] properties = playerFeatureData.Properties;
                for (int i = 0; i < properties.Length; i++)
                {
                    PlayerFeatureProperty property = properties[i];
                    if (_dicPlayerFeature.ContainsKey(property))
                    {
                        continue;
                    }
                    Type featureType = GetPlayerFeatureType(property);
                    if (gameObject.TryGetComponent(featureType, out var feature))
                    {
                        if(feature is PlayerFeature playerFeature)
                        {
                            playerFeature.Owner = _playerController;
                            playerFeature.InitAutoInEnditor();
                            _dicPlayerFeature.TryAdd(property, playerFeature);
                        }
                        continue;
                    }
                    var addedFeature = Undo.AddComponent(gameObject, featureType);
                    if (addedFeature is PlayerFeature addedPlayerFeature)
                    {
                        addedPlayerFeature.Owner = _playerController;
                        addedPlayerFeature.InitAutoInEnditor();
                        _dicPlayerFeature.Add(property, addedPlayerFeature);
                    }
                }
            }
        }

        private void ClearPlayerFeature()
        {
            foreach(var property in _dicPlayerFeature.Keys)
            {
                Type featureType = GetPlayerFeatureType(property);
                if(gameObject.TryGetComponent(featureType, out _))
                {
                    Undo.DestroyObjectImmediate(_dicPlayerFeature[property]);
                }
            }
            _dicPlayerFeature.Clear();
        }

        public Type GetPlayerFeatureType(PlayerFeatureProperty property)
        {
            return property switch
            {
                PlayerFeatureProperty.Movement => typeof(PlayerMovement),
                PlayerFeatureProperty.Rotation => typeof(PlayerRotation),
                PlayerFeatureProperty.Equipment => typeof(PlayerEquipment),
                PlayerFeatureProperty.WayPoint => typeof(PlayerWayPoint),
                _ => null
            };
        }


    }
}
