using System;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.SurroundPos
{
    [Serializable]
    public class SurroundPosContext
    {
        [SerializeField]
        private float _surroundRadius;
        [SerializeField]
        private float _surroundHeight;

        public float SurroundRadius => _surroundRadius;
        public float SurroundHeight => _surroundHeight;
    }
}
