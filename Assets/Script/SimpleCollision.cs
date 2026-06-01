using UnityEngine;

public class SimpleCollision : MonoBehaviour
{
    [SerializeField]
    private Transform _other;
    [SerializeField]
    private float _myRadius;
    [SerializeField]
    private float _otherRadius;

    private bool IsOverlapping()
    {
        if (!_other)
            return false;
        float distance = (_other.position - transform.position).sqrMagnitude;
        float radiusSum = _myRadius + _otherRadius;
        float distanceRadiusCenter = radiusSum * radiusSum;
        return distance <= distanceRadiusCenter;
    }
    private void OnDrawGizmos()
    {
        if (_other == null)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, _myRadius);
            return;
        }

        bool isOverlapping = IsOverlapping();

        Gizmos.color = isOverlapping ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, _myRadius);
        Gizmos.DrawWireSphere(_other.position, _otherRadius);
        Gizmos.DrawLine(transform.position, _other.position);
    }
}
