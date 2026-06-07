using UnityEngine;

public class SimpleCollision2 : MonoBehaviour
{
    [SerializeField]
    private Transform _other;
    [SerializeField]
    private Vector2 _myRadius;
    [SerializeField]
    private Vector2 _otherRadius;

    private bool IsOverlapping()
    {
        if (!_other)
            return false;
        Vector3 otherPosXZ = new Vector3(_other.position.x, 0f, _other.position.z);
        Vector3 myPosXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 otherPosY = new Vector3(0f, _other.position.y, 0f);
        Vector3 myPosY = new Vector3(0f, transform.position.y, 0f);
        float distanceXZ = (otherPosXZ - myPosXZ).sqrMagnitude;
        float distanceY = (otherPosY - myPosY).sqrMagnitude;
        
        float radiusSumX = _myRadius.x + _otherRadius.x;
        float distanceRadiusXZ = radiusSumX * radiusSumX;
        float radiusSumY = _myRadius.y + _otherRadius.y;
        float distanceRadiusY = radiusSumY * radiusSumY;
        if (distanceRadiusY > 0.5f)
        {
            return distanceXZ<= distanceRadiusXZ && distanceY <= distanceRadiusY;
        }
        else
        {
            return distanceXZ <= distanceRadiusXZ;
        }
    }

    private void OnDrawGizmos()
    {
        bool isOverlapping = IsOverlapping();
        Gizmos.color = isOverlapping ? Color.red : Color.green;

        Gizmos.DrawWireCube(transform.position, new Vector3(_myRadius.x, _myRadius.y, _myRadius.x));
        Gizmos.DrawWireCube(_other.position, new Vector3(_otherRadius.x, _otherRadius.y, _otherRadius.x));
        Gizmos.DrawLine(_other.position, transform.position);
    }
}
