using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
using NaughtyAttributes.Editor;
#endif

namespace KoiAI.Health
{
    [CustomEditor(typeof(SeparatedDeath))]
    public class SeparatedDeathEditor : NaughtyInspector
    {
        private const string _separatedObjPrefabPath = "Assets/_KoiAI_Project/Assets/Prefab/Particle/Pt_SeparatedObj.prefab";
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ParticleSystem separatedObjPrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(_separatedObjPrefabPath);
            SerializedProperty serializedProperty = serializedObject.FindProperty("_separatedObjPrefab");
            serializedProperty.objectReferenceValue = separatedObjPrefab;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            base.OnInspectorGUI();
        }
    }
    
    public struct SeparatedObjData
    {
        public float Duration;
        public float StartLifeTime;
        public float StartSpeed_Min;
        public float StartSpeed_Max;
        public float StartSize;
        public float StartAngle;
        public float Gravity;
        public Vector3 StartRotation;
        public Mesh ObjMesh; 
        public Material ObjMaterial;

    }

    public class SeparatedDeath : Death
    {
        [ReadOnly]
        [SerializeField]
        private ParticleSystem _separatedObjPrefab;
        [SerializeField]
        private SeparatedObjData _separatedObjData;

        public override void Initialize()
        {
        }

        public override void OnDeath(Health health)
        {
        }
    }
}
