using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.AI
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(AISystem))]
    public class AISystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            serializedObject.Update();

            SerializedProperty aiBrains = serializedObject.FindProperty("_aiBrains");
            EditorGUILayout.PropertyField(aiBrains);
            
            EditorGUILayout.HelpBox("Bake AIBrains 버튼을 누를 경우 AIBrain들을 가져오게 됩니다.", MessageType.Info);
            if (GUILayout.Button("Bake AIBrains"))
            {
                AISystem aiSystem = (AISystem)target;
                aiSystem.BakeAIBrains();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Clear NULL AIBrains"))
            {
                AISystem aiSystem = (AISystem)target;
                aiSystem.ClearNullAIBrains();
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.EndVertical();
        }
    }
    #endif
    
    /// <summary>
    /// AI들의 심장 역할을 하는 하나의 시스템
    /// </summary>
    public class AISystem : MonoBehaviour
    {
        [SerializeField] 
        private List<AIBrain> _aiBrains;
        
        internal void BakeAIBrains()
        {
            AIBrain[] aiBrains = FindObjectsByType<AIBrain>();
            if (aiBrains != null && aiBrains.Length > 0)
            {
                _aiBrains.AddRange(aiBrains);
                _aiBrains = _aiBrains.Distinct().ToList();
            }
        }

        internal void ClearNullAIBrains()
        {
            _aiBrains.RemoveAll(x => x == null);
        }
        
        private void Awake()
        {
            for (int i = 0; i < _aiBrains.Count; i++)
            {
                AIBrain aiBrain = _aiBrains[i];
                if (aiBrain)
                {
                    aiBrain.destroyCancellationToken.Register(() =>
                    {
                        _aiBrains.Remove(aiBrain);
                    });
                    aiBrain.AwakeAIBrain();    
                }
            }
        }

        private void Start()
        {
            for (int i = 0; i < _aiBrains.Count; i++)
            {
                if (_aiBrains[i])
                {
                    _aiBrains[i].StartAIBrain();
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < _aiBrains.Count; i++)
            {
                if (_aiBrains[i])
                {
                    _aiBrains[i].UpdateAIBrain();
                }
            }
            ClearNullAIBrains();
        }
    }
}
