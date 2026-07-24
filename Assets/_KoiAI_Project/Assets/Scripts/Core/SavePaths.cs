using System.IO;
using UnityEngine;

namespace KoiAI.Core
{
    public static class SavePaths
    {
        private static string _characterSettingPath = "Character-Setting.json";

        private static string GetSavePath(string savePath)
        {
            savePath = Path.Combine(Application.persistentDataPath, savePath);
            return savePath;
        }

        public static string CharacterSettingPath => GetSavePath(_characterSettingPath);
    }
}
