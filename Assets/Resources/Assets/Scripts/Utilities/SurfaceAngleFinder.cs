using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SurfaceAngleFinder
{
    private float _surfaceCheckDistance = 3f;
    
    public SurfaceAngleFinder(float surfaceCheckDistance = 3f)
    {
        _surfaceCheckDistance = surfaceCheckDistance;
    }

    public bool TryGetSurfaceAngle3D(out Vector3 angleVec,Transform rayTransform, int layerMask)
    {
        bool bGet = Physics.Raycast(rayTransform.position, rayTransform.up * -1,out RaycastHit hit, _surfaceCheckDistance, layerMask);
        angleVec = Vector3.zero;
        if (bGet)
        {
            
            Vector3 localNormal = rayTransform.InverseTransformDirection(hit.normal);
            
            float angleX = Mathf.Atan2(localNormal.z, localNormal.y) * Mathf.Rad2Deg;
            float angleZ = -Mathf.Atan2(localNormal.x, localNormal.y) * Mathf.Rad2Deg;
            angleVec.x = angleX;
            angleVec.z = angleZ;
            //Quaternion quat = Quaternion.FromToRotation(rayTransform.up, localNormal);
           // angleVec = quat.eulerAngles;
        }
        return bGet;
    }
    public bool TryGetSurfaceAngle2D(out Vector3 angleVec, Transform rayTransform, int layerMask)
    {
        bool bGet = Physics.Raycast(rayTransform.position, rayTransform.up * -1, out RaycastHit hit, _surfaceCheckDistance, layerMask);
        angleVec = Vector3.zero;
        if (bGet)
        {
            Quaternion quat = Quaternion.FromToRotation(rayTransform.up,hit.normal ) * rayTransform.rotation;
             angleVec = quat.eulerAngles;
        }
        return bGet;
    }

    public bool TryGetSurfaceAngle2D(out Vector3 angleVec, Transform rayTransform)
    {
        return TryGetSurfaceAngle2D(out angleVec, rayTransform, Physics.AllLayers);
    }

    public bool TryGetSurfaceAngle3D(out Vector3 angleVec,Transform rayTransform)
    {
        return TryGetSurfaceAngle3D(out angleVec, rayTransform, Physics.AllLayers);
    }
    
}
