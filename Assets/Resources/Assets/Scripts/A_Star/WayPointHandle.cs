using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WayPointHandle : MonoBehaviour
{
    [SerializeField]
    private PoolSize _wayPointPoolSize;
    [SerializeField]
    private int _gridWidth = 10000;
    [SerializeField]
    private int _gridHeight = 10000;
    [SerializeField]
    private GameObject _wayPointPrefab;
    [Range(0, 10)]
    [SerializeField]
    private int _wayPointStep;
    
    private AStarLogic _aStartLogic;
    private bool _isBuilding = false;
    private List<GameObject> _wayPointList;
    private Pool<GameObject> _pool;
    public void InitStartAndGoal(Vector3 start, Vector3 goal)
    {
        if(_wayPointList == null)
        {
            _wayPointList = new();
        }
        if(_aStartLogic == null)
        {
            _aStartLogic = new();
            AStarObstacle[] aStarObstacles = FindObjectsByType<AStarObstacle>();
            _aStartLogic.Initialize(aStarObstacles, _gridWidth, _gridHeight);
        }
        if (_pool == null)
        {
            var entityID = gameObject.GetEntityId();
            ulong id = EntityId.ToULong(entityID);
            PoolManager.Instance.AddPool(id, _wayPointPrefab, _wayPointPoolSize, PoolName.WayPoint);
            PoolManager.Instance.TryGetPool(id, out _pool);  
        }
        _aStartLogic.SetStartAndGoal(start, goal);
    }

    public void BuildOrClearWayPoint(bool isVisible)
    {
        if(isVisible)
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
        if (_aStartLogic == null || _wayPointList == null || _pool == null || _isBuilding)
        {
            return;
        }

        _isBuilding = true;
        ClearWayPoint();
        StartCoroutine(_aStartLogic.IERebuildPath(OnEndBuild));
    }
    
    private void OnEndBuild(List<Vector2Int> paths)
    {
        if(paths != null)
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
        for (int i = 0; i < _wayPointList.Count; i++)
        {
            _pool.Return(_wayPointList[i]);
        }
        _wayPointList.Clear();
        _isBuilding = false;
    }
    
    public bool IsBuilding => _isBuilding;
}
