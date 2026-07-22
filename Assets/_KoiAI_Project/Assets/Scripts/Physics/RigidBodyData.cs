using System;
using UnityEngine;

namespace KoiAI.CustomPhysics
{
    [Serializable]
    public struct RigidbodyData 
    {
        [SerializeField]
        private float _mass;
        [SerializeField]
        private float _linearDamping;
        [SerializeField]
        private float _angularDamping;

        public float Mass => _mass;
        public float LinearDamping => _linearDamping;
        public float AngularDamping => _angularDamping;
    }
}
