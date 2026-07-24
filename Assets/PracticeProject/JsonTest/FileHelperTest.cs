using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class FileHelperTest
{
    public static void SaveToJSON<T>(List<T> listToSave, string fileName)
    {
        string filePath = GetFilePath(fileName);
        Debug.Log(filePath);
        string jsonData = JsonHelperTest.ToJson(listToSave.ToArray(), true);
        WriteToFile(filePath, jsonData);
    }

    private static string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }

    private static void WriteToFile(string filePath, string contentToSave)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(contentToSave);
        }
    }

    public static List<T> ReadFromJSON<T>(string fileName)
    {
        string filePath = GetFilePath(fileName);
        string fileJson = ReadFromJSON(filePath);

        if(!string.IsNullOrEmpty(fileJson))
        {
            return JsonHelperTest.FromJson<T>(fileJson).ToList();
        }
        return null;
    }

    private static string ReadFromJSON(string filePath)
    {
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return reader.ReadToEnd();
            }
        }
        return string.Empty;
    }
}
