using UnityEngine;

public class GraphGizmoVisualizer : MonoBehaviour
{
    [SerializeField]
    private Vector3[] _nodePositions =
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(2f, 0f, 1f),
        new Vector3(4f, 0f, 0f),
        new Vector3(2f, 0f, -2f),
    };

    [SerializeField]
    private Vector2Int[] _edges =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 2),
        new Vector2Int(1, 3),
        new Vector2Int(3, 2),
    };

    [SerializeField]
    private float _nodeRadius = -0.2f;

    private void OnDrawGizmos()
    {
        if (_nodePositions == null)
            return;
        DrawEdges();
        DrawNodes();
    }

    private void DrawNodes()
    {
        for (int i = 0; i < _nodePositions.Length; i++)
        {
            Vector3 worldPosition = transform.position + _nodePositions[i];
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(worldPosition, _nodeRadius);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(worldPosition, _nodeRadius + 0.04f);
        }
    }

    private void DrawEdges()
    {
        if (_edges == null)
            return;
        Gizmos.color = Color.white;
        foreach (Vector2Int edge in _edges)
        {
            if (!IsValidNodeIndex(edge.x) || !IsValidNodeIndex(edge.y))
                continue;
            Vector3 from = transform.position + _nodePositions[edge.x];
            Vector3 to = transform.position + _nodePositions[edge.y];
            Gizmos.DrawLine(from, to);
        }
    }

    private bool IsValidNodeIndex(int index)
    {
        return index >= 0 && index < _nodePositions.Length;
    }
}
