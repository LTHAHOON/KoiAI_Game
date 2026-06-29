using NaughtyAttributes;
using System;
using UnityEngine;

namespace KoiAI.Physics
{
    [Serializable]
    public struct GravityData
    {
        [SerializeField]
        private bool _initOnAwake;

        [SerializeField]
        [Header("Ground Layer")]
        private LayerMask _groundLayerMask;
        [Header("Ground Check Distance")]
        [SerializeField]
        private float _groundCheckDistance;
        [Header("중력량")]
        [SerializeField]
        private float _gravity;
        [Header("중력 보정값")]
        [SerializeField]
        private float _gravityMod;
        [Header("떨어질때 목표 위치로 돌아가는 속도")]
        [SerializeField]
        private float _speedToFeetPos;
        [Tooltip("부드러운 착지를 할 때 쓰입니다.")]
        [Header("착지 시 Lerp 사용")]
        [SerializeField]
        private bool _canUseSmoothGravity;
        
        [EnableIf(nameof(_initOnAwake))]
        [AllowNesting]
        [Space(10)]
        [SerializeField]
        private Transform _feetPoint;

        public void SetFeetPoint(Transform feetPoint)
        {
            _feetPoint = feetPoint;
        }

        public bool IsInitOnAwake => _initOnAwake;
        public LayerMask GroundLayerMask => _groundLayerMask;
        public float GroundCheckDistance => _groundCheckDistance;
        public float Gravity => _gravity;
        public float GravityMod => _gravityMod;
        public float SpeedToFeetPos => _speedToFeetPos;
        public bool CanUseSmoothGravity => _canUseSmoothGravity;
        public Transform FeetPoint => _feetPoint;
    }
}
