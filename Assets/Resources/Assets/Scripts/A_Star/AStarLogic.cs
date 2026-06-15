using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AStarLogic
{
    private float _width;
    private float _height;
    private Vector2Int _start;
    private Vector2Int _goal;
    private readonly int _maxIterationCount = 5000;
    private readonly List<Vector2Int> _path = new();
    private readonly List<Vector2Int> _neighbors = new();
    private readonly HashSet<Vector2Int> _closedSet = new();
    private AStarObstacle[] _aStarObstacles;
    public void Initialize(AStarObstacle[] aStarObstacle, float gridWidth, float gridHeight)
    {
        _aStarObstacles = aStarObstacle;
        _width = gridWidth;
        _height = gridHeight;
    }

    public void SetStartAndGoal(Vector3 startPosition, Vector3 goalPosition)
    {
        _start = new Vector2Int(Mathf.RoundToInt(startPosition.x), Mathf.RoundToInt(startPosition.z));
        _goal = new Vector2Int(Mathf.RoundToInt(goalPosition.x), Mathf.RoundToInt(goalPosition.z));
    }

    public List<Vector2Int> RebuildPath()
    {
        _path.Clear();
        _neighbors.Clear();
        _closedSet.Clear();
        List<Vector2Int> openSet = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gCost = new();
        int curIterationCount = 0;

        openSet.Add(_start);
        gCost.Add(_start, 0);
        while(openSet.Count > 0)
        {
            ++curIterationCount;
            if(curIterationCount > _maxIterationCount)
            {
                break;
            }
            Vector2Int currentNode = GetFLowestCostNode(openSet, gCost);
            _closedSet.Add(currentNode);
            openSet.Remove(currentNode);
            if (_goal == currentNode)
            {
                BuildPath(currentNode, cameFrom);
                return _path;
            }


            List<Vector2Int> neighborList = GetNeighbors(currentNode);
            for (int i = 0; i < neighborList.Count; i++)
            {
                var neighbor = neighborList[i];
                if (!IsInsideGrid(neighbor) || IsInsideObstacle(neighbor))
                {
                    continue;
                }
                if(!_closedSet.Contains(neighbor))
                {
                    if(!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    gCost[neighbor] =  gCost[currentNode] + 10;
                    cameFrom[neighbor]= currentNode;
                }
            }

            _neighbors.Clear();
        }
        return null;
    }

    private IEnumerator IEBuildPath()
    {

        yield return null;
    }

    private void BuildPath(Vector2Int currentNode, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        Vector2Int current = currentNode;
        while(cameFrom.ContainsKey(current))
        {
            _path.Add(current);
            current = cameFrom[current];
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int currentNode)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.right,
        };
        for (int i = 0; i < dirs.Length; i++)
        {
            var pos = currentNode + dirs[i];
            _neighbors.Add(pos);
        }
        return _neighbors;
    }

    private Vector2Int GetFLowestCostNode(List<Vector2Int> openSet, Dictionary<Vector2Int, float> gCost)
    {
        Vector2Int bestNode = openSet[0];
        float curFCost = GetFCost(bestNode, gCost);
        for (int i = 1; i < openSet.Count; i++)
        {
            float otherFCost = GetFCost(openSet[i], gCost);
            if(curFCost > otherFCost)
            {
                curFCost = otherFCost;
                bestNode = openSet[i];
            }
        }
        return bestNode;
    }

    private float GetFCost(Vector2Int node, Dictionary<Vector2Int, float> gCost)
    {
        float h = GetMattenDistance(node, _goal);
        float g = gCost[node];
        float f = h + g;

        return f;
    }


    private float GetMattenDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool IsInsideObstacle(Vector2Int node)
    {
        if(_aStarObstacles == null)
        {
            return false;
        }
        for (int i = 0; i < _aStarObstacles.Length; i++)
        {
            bool isInside = _aStarObstacles[i].IsInsideObstacle(node);
            if(isInside)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsInsideGrid(Vector2Int node)
    {
        return node.x >= -_width/2 && node.x <= _width/2 && node.y >= -_height/2 && node.y <= _height/2;
    }
}
