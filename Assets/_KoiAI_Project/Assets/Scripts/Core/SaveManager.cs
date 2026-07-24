
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

namespace KoiAI.Core
{
    public class SaveWrapper<T>
    {
        public T[] Values { get; set; }

        public SaveWrapper(T[] values)
        {
            Values = values;
        }
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// 세이브 파일 생성하기
        /// </summary>
        public void SaveToJson<T>(List<T> objs, string savePath) where T : class
        {
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            SaveWrapper<T> wrapper = new SaveWrapper<T>(objs.ToArray());

            string serializedJson = JsonUtility.ToJson(wrapper, true);
            FileStream fileStream = new(savePath, FileMode.Create);
            using (StreamWriter writer = new(fileStream))
            {
                writer.Write(serializedJson);
            }
            Debug.Log($"세이브 파일 생성완료: {savePath}");
        }

        /// <summary>
        /// 세이브 파일 생성및 저장 하기
        /// </summary>
        public void SaveToJson<T>(T obj, string savePath) where T : class
        {
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            string serializedJson = JsonUtility.ToJson(obj, true);
            FileStream fileStream = new(savePath, FileMode.Create);
            using (StreamWriter writer = new(fileStream))
            {
                writer.Write(serializedJson);
            }
            Debug.Log($"세이브 파일 생성 및 저장완료: {savePath}");
        }

        /// <summary>
        /// 세이브 파일 가져와서 덮어쓰기
        /// </summary>
        public void SaveFromJson<T>(List<T> objs, string savePath) where T : class
        {
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }
            SaveWrapper<T> wrapper = new SaveWrapper<T>(objs.ToArray());

            string serializedJson = ReadSaveFile<T>(savePath);
            JsonUtility.FromJsonOverwrite(serializedJson, wrapper);
            Debug.Log($"세이브 파일 덮어쓰기 완료: {savePath}");
        }

        /// <summary>
        /// 세이브 파일 가져와서 덮어쓰기
        /// </summary>
        public void SaveFromJson<T>(T obj, string savePath) where T : class
        {
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }
            string serializedJson = ReadSaveFile<T>(savePath);
            JsonUtility.FromJsonOverwrite(serializedJson, obj);
            Debug.Log($"세이브 파일 덮어쓰기 완료: {savePath}");
        }

        public string ReadSaveFile<T>(string savePath) where T : class
        {
            if (File.Exists(savePath))
            {
                using (StreamReader reader = new(savePath))
                {
                    string serializedJson = reader.ReadToEnd();
                    return serializedJson;
                }
            }
            return null;
        }


    }
}
