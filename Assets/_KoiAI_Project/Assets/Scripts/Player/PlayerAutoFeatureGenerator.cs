using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using System;
using System.Linq;
using static KoiAI.Player.PlayerFeature;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.Player
{
    public class PlayerAutoFeatureGenerator : MonoBehaviour
    {
        [InfoBox("플레이어 데이터를 바꾸게 되면 생성 버튼을 눌려주시고 \n " +
                 "Player CMData Mediator 인스펙터에서 Connect 버튼을 눌려주세요", EInfoBoxType.Normal)]
        [HorizontalLine(5, EColor.Gray)]
        
        [SerializeField]
        private PlayerController _playerController;

        private PlayerData _playerData;
        private readonly Dictionary<PlayerFeatureProperty, PlayerFeature> _dicPlayerFeature = new();

        [Button("(Re)Generate Feature Component" , EButtonEnableMode.Editor)]
        public void GeneratePlayerFeature()
        {
            if (_playerController == null)
            {
                Debug.Log("Failed Generate: PlayerController is null");
                return;
            }
            _playerController.InitInEditor();
            _playerData = _playerController.PlayerData;

            ClearPlayerFeature();
            AddPlayerFeature();

            Debug.Log("Completed Generate Player Feature");
        }

#if UNITY_EDITOR
        private void AddPlayerFeature()
        {
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            if (playerFeatureData)
            {
                PlayerFeatureProperty[] properties = playerFeatureData.Properties;
                for (int i = 0; i < properties.Length; i++)
                {
                    PlayerFeatureProperty property = properties[i];
                    bool success = false;
                    if (_dicPlayerFeature.ContainsKey(property))
                    {
                        _dicPlayerFeature[property].InitAutoInEnditor();
                        Debug.Log("Already Exist:");
                        continue;
                    }
                    Type featureType = GetPlayerFeatureType(property);
                    if (gameObject.TryGetComponent(featureType, out var feature))
                    {
                        if(feature is PlayerFeature playerFeature)
                        {
                            playerFeature.Owner = _playerController;
                            playerFeature.InitAutoInEnditor();
                            success  = _dicPlayerFeature.TryAdd(property, playerFeature);
                        }
                        continue;
                    }
                    var addedFeature = Undo.AddComponent(gameObject, featureType);
                    if (addedFeature is PlayerFeature addedPlayerFeature)
                    {
                        addedPlayerFeature.Owner = _playerController;
                        addedPlayerFeature.InitAutoInEnditor();
                        success= _dicPlayerFeature.TryAdd(property, addedPlayerFeature);
                    }
                    Debug.Log(success);
                }
            }
        }

        private void ClearPlayerFeature()
        {
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            PlayerFeatureProperty[] properties = playerFeatureData.Properties;
            var playerFeatureList = _dicPlayerFeature.ToList();
            foreach (var playerFeaturePair in playerFeatureList)
            {
                if (!properties.Contains(playerFeaturePair.Value.FeatureProperty))
                {
                    Undo.DestroyObjectImmediate(playerFeaturePair.Value);
                    _dicPlayerFeature.Remove(playerFeaturePair.Key);
                }
            }
        }
#endif
        
        private Type GetPlayerFeatureType(PlayerFeatureProperty property)
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
