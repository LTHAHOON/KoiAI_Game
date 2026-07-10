using System;
using UnityEngine;

namespace KoiAI.Utilities
{
    [Serializable]
    public class SceneReference
    {
        [SerializeField][HideInInspector]
        private string _scenePathRecord;
        
        public string ScenePath => _scenePathRecord;
    }
}
