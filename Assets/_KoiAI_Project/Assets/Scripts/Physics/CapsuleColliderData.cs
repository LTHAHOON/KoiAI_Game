using System;
using UnityEngine;

namespace KoiAI.Physics
{
    [Serializable]
    public struct CapsuleColliderData
    {
        [SerializeField]
        private Vector3 _center;
        [SerializeField]
        private float _radius;
        [SerializeField]
        private float _height;
        [Tooltip("높이 방향 \n 0 = X-axis \n 1 = Y-axis \n 2 = Z-axis")]
        [SerializeField]
        private int _direction;

        public Vector3 Center => _center;
        public float Radius => _radius;
        public float Height => _height;
        public int Direction => _direction;
    }
}
