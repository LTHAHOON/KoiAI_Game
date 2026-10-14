using UnityEngine;
using NaughtyAttributes;
using System;
using System.Collections.Generic;



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
    
    [Serializable]
    public struct SeparatedObjData
    {
        public Transform CreatePrefabPos;
        public float Duration;
        public float StartLifeTime;
        public float StartSpeed_Min;
        public float StartSpeed_Max;
        public float StartSize;
        public float StartAngle;
        public float Gravity;
        public Vector3 StartRotation;
        public Vector3 ShapeRotation;
        public Mesh ObjMesh; 
        public Material ObjMaterial;
        public static SeparatedObjData CreateDefault()
        {
            return new SeparatedObjData
            {
                Duration = 1f,
                StartLifeTime = 1f,
                StartSpeed_Min = 15f,
                StartSpeed_Max = 20f,
                StartSize = 0.04f,
                StartAngle = 8f,
                Gravity = 9.81f,
                ShapeRotation = new Vector3(0f, 0f, 0f)
            };
        }
    }

    public class SeparatedDeath : Death
    {
        [ReadOnly]
        [SerializeField]
        private ParticleSystem _separatedObjPrefab;
        [SerializeField]
        private List<SeparatedObjData> _separatedObjDataList = new() { SeparatedObjData.CreateDefault() };
        [SerializeField]
        private List<Transform> _collisionPlanes = new();

        private readonly List<ParticleSystem> _separatedObjList = new();

        public override void Initialize()
        {
            for (int i = 0; i < _separatedObjDataList.Count; ++i)
            {
                SeparatedObjData separatedObjData = _separatedObjDataList[i];
                ParticleSystem separatedObj = Instantiate(_separatedObjPrefab, separatedObjData.CreatePrefabPos);
                var mainModule = separatedObj.main;
                var shapeModule = separatedObj.shape;
                var collisionModule = separatedObj.collision;
                var particleRenderer = separatedObj.GetComponent<ParticleSystemRenderer>();
                
                collisionModule.type = ParticleSystemCollisionType.Planes;
                bool hasCollisionPlane = _collisionPlanes.Count > 0;
                if(hasCollisionPlane)
                {
                    for (int planeIndex = 0; planeIndex < collisionModule.planeCount; ++planeIndex)
                    {
                        Transform plane = _collisionPlanes[planeIndex];
                        collisionModule.SetPlane(planeIndex, plane);
                    }
                }

                mainModule.loop = false;
                mainModule.duration = separatedObjData.Duration;
                mainModule.startLifetime = separatedObjData.StartLifeTime;
                mainModule.startSpeed = new(separatedObjData.StartSpeed_Min, separatedObjData.StartSpeed_Max);
                mainModule.startSize = separatedObjData.StartSize;
                mainModule.gravityModifierMultiplier = separatedObjData.Gravity;
                mainModule.playOnAwake = false;
                mainModule.startRotation3D = true;
                mainModule.startRotationX = separatedObjData.StartRotation.x  * Mathf.Deg2Rad;
                mainModule.startRotationY = separatedObjData.StartRotation.y  * Mathf.Deg2Rad;
                mainModule.startRotationZ = separatedObjData.StartRotation.z  * Mathf.Deg2Rad;
                shapeModule.angle = separatedObjData.StartAngle;
                shapeModule.rotation = separatedObjData.ShapeRotation;
                particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                particleRenderer.mesh = separatedObjData.ObjMesh;
                particleRenderer.sharedMaterial = separatedObjData.ObjMaterial;
                
                _separatedObjList.Add(separatedObj);
            }
        }

        public override void OnDeath(Health health)
        {
            for (int i = 0; i < _separatedObjList.Count; ++i)
            {
                ParticleSystem separatedObj = _separatedObjList[i];
                separatedObj.Play();
            }
        }
    }
}
