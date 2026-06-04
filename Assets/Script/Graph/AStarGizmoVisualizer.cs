using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class AStarGizmoVisualizer : MonoBehaviour
{
    [SerializeField]
    private int _width = 6;
    [SerializeField]
    private int _height = 4;
    [SerializeField]
    private float _cellSize = 1f;
    [SerializeField]
    private Vector2Int _start = Vector2Int.zero;
    [SerializeField]
    private Vector2Int _goal = new Vector2Int(5, 3);
    [SerializeField]
    private Vector2Int[] _walls =
    {
        new(2, 0),
        new(2, 1),
        new(2, 2),
    };
    [SerializeField]
    private int _maxInterations = 100;

    private readonly List<Vector2Int> _path = new();
    private readonly HashSet<Vector2Int> _closedSet = new();
    private bool _pathFound;
    private bool _stoppedBySafetyLimit;

    private void OnValidate()
    {
        RebuildPath();
    }

    private void Awake()
    {
        RebuildPath();
    }

    private void RebuildPath()
    {
        _path.Clear();
        _closedSet.Clear();
        _pathFound = false;
        _stoppedBySafetyLimit = false;

        if(!IsInsideGrid(_start) || !IsInsideGrid(_goal) || IsWall(_start) || IsWall(_goal))
        {
            return;
        }

        List<Vector2Int> openSet = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> gCost = new();

        openSet.Add(_start);
        gCost[_start] = 0;

        int iterationCount = 0;

        while (openSet.Count > 0)
        {
            iterationCount++;

            //최대 루프횟수를 비교하여 무한 루프 방지
            if(iterationCount > _maxInterations)
            {
                _stoppedBySafetyLimit = true;
                return;
            }

            //F 비용이 가장 낮은 노드를 고릅니다.
            Vector2Int current = GetLowetFCostNode(openSet, gCost);

            //목표지점과 동일할 경우
            if(current == _goal)
            {
                BuildPath(cameFrom, current);
                _pathFound = true;
                return;
            }

            openSet.Remove(current);
            _closedSet.Add(current);

            //다음 루프에 F 비용 비교를 하기 위해 Current를 중심으로 근처 노드 찾아서 OpenSet에 넣어줍니다.
            foreach(Vector2Int neighbor in GetNeighbors(current))
            {
                //해당 노드 위치에 장애물이 있거나 포함된 노드일 경우 continue
                if(_closedSet.Contains(neighbor) || IsWall(neighbor))
                {
                    continue;
                }


                int newGCost = gCost[current] + 10;
                if(!gCost.ContainsKey(neighbor) || newGCost < gCost[neighbor])
                {
                    //neighbor들중 하나가 다음 루프에 Current가 되기 때문에 미리 다 현재의 Current 노드와 연결해줍니다.
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = newGCost;

                    if(!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
    }

    private void BuildPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        _path.Clear();
        _path.Add(current);

        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            _path.Add(current);
        }
        //목표점 -> 시작점 순서를 시작점 -> 목표점 순서로 만듭니다.
        _path.Reverse();
    }

    private Vector2Int GetLowetFCostNode(List<Vector2Int> openSet, Dictionary<Vector2Int,int> gCost)
    {
        //일단 첫 번째 후보를 가장 좋은 칸이라고 가정하고 시작합니다.
        Vector2Int bestNode = openSet[0];
        int bestCost = GetFCost(bestNode, gCost);

        for (int i = 1; i < openSet.Count; i++)
        {
            int cost = GetFCost(openSet[i], gCost);
            if(cost < bestCost)
            {
                bestNode = openSet[i];
                bestCost = cost;
            }
        }
        return bestNode;
    }

    private int GetFCost(Vector2Int node, Dictionary<Vector2Int, int> gCost)
    {
        //G 비용: 시작점에서 이 칸까지 실제로 이동한 비용입니다.
        int g = gCost.ContainsKey(node) ? gCost[node] : 9999;

        //H 비용: 이 칸에서 목표까지 남았다고 예상하는 비용입니다.
        int h = GetManhattanDistance(node, _goal) * 10;

        return g + h;
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        // 두 칸 사이를 가로/세로 이동만으로 간다고 생각했을 때 필요한 최소 칸 수입니다.
        // 예를 들어 (0, 0)에서 (3, 2)로 가려면 오른쪽으로 3칸, 위로 2칸 이동해야 합니다.
        // 그래서 거리는 3 + 2 = 5칸이 됩니다.
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new();
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int direction in directions)
        {
            // 현재 칸에 방향 값을 더하면 그 방향의 이웃 칸 좌표가 됩니다.
            Vector2Int next = node + direction;

            // 격자 밖으로 나간 칸은 후보에 넣지 않습니다.
            if (IsInsideGrid(next))
            {
                neighbors.Add(next);
            }
        }

        return neighbors;

    }

    private bool IsInsideGrid(Vector2Int node)
    {
        return node.x >= 0 && node.x < _width && node.y >= 0 && node.y < _height;
    }

    private bool IsWall(Vector2Int node)
    {
        foreach(Vector2Int wall in _walls)
        {
            if(wall == node)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        // Scene 뷰가 갱신될 때마다 최신 Inspector 값으로 경로를 다시 계산합니다.
        RebuildPath();

        // y와 x를 돌며 모든 격자 칸을 하나씩 그립니다.
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Vector2Int node = new Vector2Int(x, y);

                // 2D 격자 좌표를 Unity 월드 좌표로 바꿉니다. y 높이는 0으로 고정합니다.
                Vector3 position = transform.position + new Vector3(x * _cellSize, 0f, y * _cellSize);

                // 칸의 상태에 따라 색을 고르고, 색칠된 정육면체를 그립니다.
                Gizmos.color = GetNodeColor(node);
                Gizmos.DrawCube(position, Vector3.one * (_cellSize * 0.85f));

                // 칸 경계를 검은 선으로 한 번 더 그려서 격자 구조가 잘 보이게 합니다.
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(position, Vector3.one * (_cellSize * 0.85f));
            }
        }

        // 경로를 찾았다면 칸 중심을 잇는 흰색 선을 추가로 그립니다.
        DrawPathLines();
    }

    private Color GetNodeColor(Vector2Int node)
    {
        // 시작점은 가장 먼저 눈에 띄도록 초록색으로 표시합니다.
        if (node == _start)
        {
            return Color.green;
        }

        // 목표점은 빨간색으로 표시합니다.
        if (node == _goal)
        {
            return Color.red;
        }

        // 벽은 지나갈 수 없는 칸이므로 검은색으로 표시합니다.
        if (IsWall(node))
        {
            return Color.black;
        }

        // 최종 경로에 포함된 칸은 노란색으로 표시합니다.
        if (_path.Contains(node))
        {
            return Color.yellow;
        }

        // 확인이 끝난 칸은 하늘색으로 표시합니다.
        // 안전장치에 걸려 중단된 경우에는 보라색으로 표시해 실패 원인을 구분합니다.
        if (_closedSet.Contains(node))
        {
            return _stoppedBySafetyLimit ? Color.magenta : Color.cyan;
        }

        // 아직 탐색에서 특별한 의미가 없는 기본 칸입니다.
        return Color.gray;
    }

    private void DrawPathLines()
    {
        // 경로를 찾지 못했거나 선을 이을 만큼 칸이 부족하면 아무것도 그리지 않습니다.
        if (!_pathFound || _path.Count < 2)
        {
            return;
        }

        Gizmos.color = Color.white;

        // path[0] -> path[1], path[1] -> path[2]처럼 이웃한 경로 칸끼리 선을 잇습니다.
        for (int i = 1; i < _path.Count; i++)
        {
            // 칸 중심보다 살짝 높은 위치에 선을 그려서 큐브 안에 묻히지 않게 합니다.
            Vector3 from = transform.position + new Vector3(_path[i - 1].x * _cellSize, 0.55f, _path[i - 1].y * _cellSize);
            Vector3 to = transform.position + new Vector3(_path[i].x * _cellSize, 0.55f, _path[i].y * _cellSize);
            Gizmos.DrawLine(from, to);
        }
    }
}
