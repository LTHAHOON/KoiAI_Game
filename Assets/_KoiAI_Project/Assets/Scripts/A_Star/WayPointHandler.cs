using System.Collections.Generic;
using KoiAI.Pool;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI.A_Star
{
    public class WayPointHandler
    {
        private List<GameObject> _wayPointList;
        private AStarLogic _aStartLogic;
        private Pool<GameObject> _pool;
        private MonoBehaviour _owner;
        private bool _isBuilding = false;
        private int _wayPointStep = 0;
        public void InitStartAndGoal(MonoBehaviour owner, WayPointData wayPointData, Vector3 start, Vector3 goal)
        {
            if(!owner)
            {
                return;
            }

            if(!IsCompletedInit())
            {
                _owner = owner;

                _wayPointList = new();
                _aStartLogic = new();
                _wayPointStep = wayPointData.WayPointStep;
                AStarObstacle[] aStarObstacles = Object.FindObjectsByType<AStarObstacle>();
                _aStartLogic.Initialize(aStarObstacles, wayPointData.GridWidth, wayPointData.GridHeight);

                ulong id = _owner.GetEntityULongID();
                PoolManager.Instance.AddPool(id, wayPointData.WayPointPrefab, wayPointData.WayPointPoolSize, PoolName.WayPoint);
                PoolManager.Instance.TryGetPool(id, out _pool);
            }
            _aStartLogic.SetStartAndGoal(start, goal);
        }

        public void BuildOrClearWayPoint(bool isVisible)
        {
            if (!IsCompletedInit())
            {
                return;
            }

            if (isVisible)
            {
                BuildWayPoint();
            }
            else
            {
                ClearWayPoint();
            }
        }
    
        public void BuildWayPoint()
        {
            if (!IsCompletedInit() || _isBuilding)
            {
                return;
            }

            _isBuilding = true;
            ClearWayPoint();
            
            _owner.StartCoroutine(_aStartLogic.IERebuildPath(OnEndBuild));

        }

        private void OnEndBuild(List<Vector2Int> paths)
        {
            if (!IsCompletedInit())
            {
                return;
            }

            if (paths != null)
            {
                for (int i = 0; i < paths.Count; i += _wayPointStep + 1)
                {
                    GameObject wayPoint = _pool.Pop();
                    _wayPointList.Add(wayPoint);
                    wayPoint.transform.position = new Vector3(paths[i].x, 1f, paths[i].y);

                }
            }
            else
            {
                ClearWayPoint();
            }
        }
    
        public void ClearWayPoint()
        {
            if (!IsCompletedInit())
            {
                return;
            }

            for (int i = 0; i < _wayPointList.Count; i++)
            {
                _pool.Return(_wayPointList[i]);
            }
            _wayPointList.Clear();
            _isBuilding = false;
        }

        private bool IsCompletedInit() => _owner && _aStartLogic != null && _pool != null && _wayPointList != null;
        public bool IsBuilding => _isBuilding;
    }
}
