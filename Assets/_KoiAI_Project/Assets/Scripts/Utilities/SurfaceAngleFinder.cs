using UnityEngine;

namespace KoiAI.Utilities
{
    public class SurfaceAngleFinder
    {
        private float _surfaceCheckDistance = 3f;
    
        public SurfaceAngleFinder(float surfaceCheckDistance = 3f)
        {
            _surfaceCheckDistance = surfaceCheckDistance;
        }

        public bool TryGetLocalSurfaceAngle(out Vector3 angleVec,Transform rayTransform, int layerMask)
        {
            bool bGet = Physics.Raycast(rayTransform.position,Vector3.down,out RaycastHit hit, _surfaceCheckDistance, layerMask);
            angleVec = Vector3.zero;
            if (bGet)
            {
                Vector3 localNormal = rayTransform.InverseTransformDirection(hit.normal);
                angleVec = Quaternion.FromToRotation(Vector3.up, localNormal).eulerAngles;
                /*
            float angleX = Mathf.Atan2(localNormal.z, localNormal.y) * Mathf.Rad2Deg;
            float angleZ = -Mathf.Atan2(localNormal.x, localNormal.y) * Mathf.Rad2Deg;
            angleVec.x = angleX;
            angleVec.z = angleZ;
            */
            }
            return bGet;
        }
        public bool TryGetWorldSurfaceAngle(out Vector3 angleVec,Transform rayTransform, int layerMask)
        {
            bool bGet = Physics.Raycast(rayTransform.position,Vector3.down,out RaycastHit hit, _surfaceCheckDistance, layerMask);
            if (bGet)
            {
                angleVec = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
                return true;
            }
            angleVec = Vector3.zero;
            return true;
        }
        public bool TryGetLocalSurfaceAngle(out Vector3 angleVec,Transform rayTransform)
        {
            return TryGetLocalSurfaceAngle(out angleVec, rayTransform,Physics.AllLayers);
        }

        public bool TryGetWorldSurfaceAngle(out Vector3 angleVec, Transform rayTransform)
        {
            return TryGetWorldSurfaceAngle(out angleVec, rayTransform,Physics.AllLayers);
        }
    }
}
