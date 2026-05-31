using UnityEditor.UIElements;
using UnityEngine;

public class SurfaceAngleFinder : MonoBehaviour
{
    public bool TrySurfaceAngleUsingRaycast(out Vector3 angleVec,Transform rayTransform ,float maxDistance, int layerMask)
    {
        bool bGet = Physics.Raycast(rayTransform.position, rayTransform.up * -1,out RaycastHit hit, maxDistance, layerMask);
        angleVec = new Vector3(0f, 0f, 0f);
        if (bGet)
        {
            Vector3 localNormal = rayTransform.InverseTransformDirection(hit.normal);
            float angleX = Mathf.Atan2(localNormal.z, localNormal.y) * Mathf.Rad2Deg;
            float angleZ = -Mathf.Atan2(localNormal.x, localNormal.y) * Mathf.Rad2Deg;
            angleVec.x = angleX;
            angleVec.z = angleZ;
        }
        return bGet;
    }
    
    public bool TrySurfaceAngleUsingRaycast(out Vector3 angleVec,Transform rayTransform ,float maxDistance)
    {
        return TrySurfaceAngleUsingRaycast(out angleVec, rayTransform, maxDistance, Physics.AllLayers);
    }
    
}
