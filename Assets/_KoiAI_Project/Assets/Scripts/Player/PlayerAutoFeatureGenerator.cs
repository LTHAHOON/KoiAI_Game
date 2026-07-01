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

        [SerializeField]
        private Dictionary<PlayerFeatureProperty, PlayerFeature> _dicPlayerFeature = new();
        [SerializeField]
        private HashSet<PlayerFeatureProperty> _prevPlayerFeatureProperties = new();


#if UNITY_EDITOR

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


        private void AddPlayerFeature()
        {
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            if (playerFeatureData)
            {
                PlayerFeatureProperty[] properties = playerFeatureData.Properties;
                for (int i = 0; i < properties.Length; i++)
                {
                    PlayerFeatureProperty property = properties[i];
                    Type featureType = GetPlayerFeatureType(property);
                    if (gameObject.TryGetComponent(featureType, out var feature))
                    {
                        if (_dicPlayerFeature.ContainsKey(property))
                        {
                            _dicPlayerFeature[property].InitAutoInEnditor();
                            if (!_prevPlayerFeatureProperties.Contains(property))
                            {
                                _prevPlayerFeatureProperties.Add(property);
                            }
                            continue;
                        }
                        else
                        {
                            if (feature is PlayerFeature playerFeature)
                            {
                                playerFeature.Owner = _playerController;
                                playerFeature.InitAutoInEnditor();
                                _prevPlayerFeatureProperties.Add(property);
                                _dicPlayerFeature.Add(property, playerFeature);
                            }
                        }
                    }
                    else
                    {
                        var addedFeature = Undo.AddComponent(gameObject, featureType);
                        if (addedFeature is PlayerFeature addedPlayerFeature)
                        {
                            addedPlayerFeature.Owner = _playerController;
                            addedPlayerFeature.InitAutoInEnditor();
                            _prevPlayerFeatureProperties.Add(property);
                            _dicPlayerFeature.TryAdd(property, addedPlayerFeature);
                        }
                    }
                    
                }
            }
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }

        private void ClearPlayerFeature()
        {
            PlayerFeatureData playerFeatureData = _playerData.GetPlayerFeatureData();
            PlayerFeatureProperty[] properties = playerFeatureData.Properties;
            foreach(PlayerFeatureProperty prevProperty  in _prevPlayerFeatureProperties)
            {
                if (!properties.Contains(prevProperty))
                {
                    if (_dicPlayerFeature.TryGetValue(prevProperty, out PlayerFeature feature))
                    {
                        Undo.DestroyObjectImmediate(feature);
                        _dicPlayerFeature.Remove(prevProperty);
                    }
                }
            }
            _prevPlayerFeatureProperties.Clear();
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
