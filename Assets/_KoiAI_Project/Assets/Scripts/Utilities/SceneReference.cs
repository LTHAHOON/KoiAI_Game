using System;
using UnityEngine;

namespace KoiAI.Utilities
{
    [Serializable]
    public class SceneReference
    {
        [SerializeField][HideInInspector]
        private string _scenePathRecord;
        
        public SceneReference(string scenePath)
        {
            _scenePathRecord = scenePath;
        }
        
        public string ScenePath => _scenePathRecord;
    }
}
