using KoiAI.Pool;
using System;
using UnityEngine;

namespace KoiAI.A_Star
{
    [CreateAssetMenu(fileName = "new WayPointData", menuName = "KoiAI/AStar/WayPointData")]
    public class WayPointData : ScriptableObject
    {
        [SerializeField]
        private PoolSize _wayPointPoolSize;
        [SerializeField]
        private int _gridWidth = 5000;
        [SerializeField]
        private int _gridHeight = 5000;
        [SerializeField]
        private GameObject _wayPointPrefab;
        [Range(0, 10)]
        [SerializeField]
        private int _wayPointStep;

        public PoolSize WayPointPoolSize => _wayPointPoolSize;
        public int GridWidth => _gridWidth; 
        public int GridHeight => _gridHeight;
        public GameObject WayPointPrefab => _wayPointPrefab;
        public int WayPointStep => _wayPointStep;   
    }
}
