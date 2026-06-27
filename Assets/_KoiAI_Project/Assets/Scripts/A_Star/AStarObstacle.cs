using UnityEngine;

namespace KoiAI.A_Star
{
    [RequireComponent (typeof(Collider))]
    public class AStarObstacle : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [SerializeField]
        private Vector3 _scale = Vector3.one;

        private Collider _collider;
        private Bounds _bounds;
        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _bounds = _collider.bounds;
            _bounds.size = new Vector3(_bounds.size.x * _scale.x, _bounds.size.y * _scale.y, _bounds.size.z * _scale.z);
            _bounds.center = _bounds.center + _offset;
        }
        public bool IsInsideObstacle(Vector2Int node)
        {
            Vector3 nodeVec3 = new Vector3(node.x, 1f, node.y);
            if(_bounds.Contains(nodeVec3))
            {
                return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
        }
    }
}
