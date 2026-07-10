using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace KoiAI.Edtior
{
    using KoiAI.Utilities;
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        //밑에 높이 설정
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3f;
        }
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty scenePathRecordProperty = property.FindPropertyRelative("_scenePathRecord");
            SceneAsset curSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathRecordProperty.stringValue);
            EditorGUI.BeginChangeCheck();
            
            Rect objectRect = new Rect(position.x, position.y + 7f, position.width, EditorGUIUtility.singleLineHeight);
            SceneAsset newSceneAsset = EditorGUI.ObjectField(objectRect, label, curSceneAsset, typeof(SceneAsset), false) as SceneAsset;
            
            EditorGUI.BeginDisabledGroup(true);
            Rect rect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 7f, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextField(rect, "Scene Path", scenePathRecordProperty.stringValue);
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck())
            {
                if (newSceneAsset)
                {
                    scenePathRecordProperty.stringValue = AssetDatabase.GetAssetPath(newSceneAsset);
                }
                else
                {
                    scenePathRecordProperty.stringValue = "";
                }
            }
            EditorGUI.EndProperty();
            
        }
    }
}
