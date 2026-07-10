using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace KoiAI.Editor
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

            //indent: 들여쓰기
            EditorGUI.indentLevel = 0;
            
            Rect objectRect = new Rect(position.x, position.y + 7f, position.width, EditorGUIUtility.singleLineHeight);
            SceneAsset newSceneAsset = EditorGUI.ObjectField(objectRect, label, curSceneAsset, typeof(SceneAsset), false) as SceneAsset;
            
            EditorGUI.BeginDisabledGroup(true);
            Rect textRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 10f, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextField(textRect, "Scene Path", scenePathRecordProperty.stringValue);
            EditorGUI.EndDisabledGroup();

            Rect rect = new(position.x, position.y + EditorGUIUtility.singleLineHeight + 16f, position.width, EditorGUIUtility.singleLineHeight);

            // FocusType중에서 Passive와 Keyboard가 있습니다.
            // Keyboard는 키보드로 글자를 입력받을수있는 ID이고 Passive는 그 반대입니다.(Tab키를 누르면 이해할 수 있습니다.)
            //Rect rect2 = new(position.x, position.y + EditorGUIUtility.singleLineHeight + 21f, position.width, EditorGUIUtility.singleLineHeight);
            //int id = GUIUtility.GetControlID(FocusType.Keyboard);
            //Rect dataPosition  = EditorGUI.PrefixLabel(rect, id, new("Hello"));

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
