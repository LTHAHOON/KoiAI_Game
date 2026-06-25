using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DfsGizmoVisualizer : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("가로 칸 수 입니다.")]
    [SerializeField]
    private int _width = 3;
    [Tooltip("세로 칸 수 입니다.")]
    [SerializeField]
    private int _height = 3;
    [SerializeField]
    private float _cellSize = 1f;

    private readonly Stack<Vector2Int> _frontier = new();
    private readonly HashSet<Vector2Int> _visited = new();
    private Vector2Int _currentNode;

    private void Start()
    {
        ResetSearch();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StepSearch();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetSearch();
        }
    }

    private void ResetSearch()
    {
        _frontier.Clear();
        _visited.Clear();

        _currentNode = new Vector2Int(0, 0);
        _frontier.Push(_currentNode);
        _visited.Add(_currentNode);
    }

    private void StepSearch()
    {
        if(_frontier.Count == 0)
        {
            return;
        }

        _currentNode = _frontier.Pop();

        foreach(Vector2Int neighbor in GetNeighbors(_currentNode))
        {
            if(_visited.Contains(neighbor))
            {
                continue;
            }
            _visited.Add(neighbor);
            _frontier.Push(neighbor);
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new();
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        foreach(Vector2Int direction in directions)
        {
            Vector2Int next = node + direction;
            if(next.x < 0 || next.x >= _width || next.y < 0 || next.y >= _height)
            {
                continue;
            }
            neighbors.Add(next);
        }
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Vector2Int node = new Vector2Int(x, y);
                Vector3 position = transform.position + new Vector3(x * _cellSize, 0f, y * _cellSize);

                Gizmos.color = GetNodeColor(node);
                Gizmos.DrawCube(position, Vector3.one * (_cellSize * 0.8f));

                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(position, Vector3.one * (_cellSize * 0.8f));
            }
        }
    }

    private Color GetNodeColor(Vector2Int node)
    {
        if (Application.isPlaying && node == _currentNode)
        {
            return Color.yellow;
        }

        if (Application.isPlaying && _visited.Contains(node))
        {
            return Color.magenta;
        }

        return Color.gray;
    }
}
