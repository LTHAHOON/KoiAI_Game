using UnityEngine;

namespace KoiAI
{
    public class AimCameraPivot : MonoBehaviour
    {
        [SerializeField] 
        private Transform _player;
        [SerializeField] 
        private Transform _aimCameraPivot;

        [SerializeField] 
        private float _pivotHeight = 2f;

        private float _yaw;
        private float _pitch;

        private void Update()
        {
            UpdateAimPivot();
        }

        private void UpdateAimPivot()
        {
            _aimCameraPivot.SetPositionAndRotation(_player.position + Vector3.up * _pivotHeight, Quaternion.Euler(_pitch, _yaw, 0f));
        }
    }
}
