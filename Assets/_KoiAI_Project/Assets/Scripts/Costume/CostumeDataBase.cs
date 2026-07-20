
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KoiAI.Costume
{
    [CreateAssetMenu(fileName = "new CostumeDataBase", menuName = "KoiAI/Costume/CostumeDataBase")]
    public class CostumeDataBase : ScriptableObject
    {
        [SerializeField]
        private CustomeCategory _costumeCategory;

        [SerializeField]
        private List<CostumeData> _costumeDataList;

#if UNITY_EDITOR
        [Button("LoadCostumeData", EButtonEnableMode.Editor)]
        public void LoadCostumeData()
        {
            _costumeDataList.Clear();
            GUID[] costumeDataGuids = AssetDatabase.FindAssetGUIDs("t: ScriptableObject", new[] { "Assets/_KoiAI_Project/Assets/ScriptableObject/Costume/CostumeData" });
            for (int i = 0; i < costumeDataGuids.Length; i++)
            {
                CostumeData costumData = AssetDatabase.LoadAssetByGUID<CostumeData>(costumeDataGuids[i]);
                if (costumData)
                {
                    if(costumData.CostumeCategory == _costumeCategory)
                    {
                        _costumeDataList.Add(costumData);
                    }
                }
            }
        }
#endif
    }
}
