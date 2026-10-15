using UnityEngine;

namespace KoiAI.Item
{
    [CreateAssetMenu(fileName = "new GunData", menuName = "WeaponData/GunData")]
    public class GunData : WeaponData
    {
        [SerializeField]
        private LayerMask _layerMaskForAim;
        [SerializeField]
        private BulletData _bulletData;
        [SerializeField]
        [Tooltip("예비 탄약만 무한입니다. 탄창의 탄약은 소모되며 재장전이 필요합니다.")]
        private bool _isInfiniteLoad = false;
        [SerializeField]
        [Tooltip("탄창이 비었을 때 자동으로 재장전합니다. false이면 수동 재장전만 가능합니다.")]
        private bool _isAutoLoad = false;
        [SerializeField]
        private float _aimSensitity = 3f;
        [SerializeField]
        private int _maxHitCount = 1;
        [SerializeField]
        private int _loadMaxCount = 20;
        [SerializeField]
        private float _launchSpeed = 12f;
        [SerializeField]
        [Tooltip("빈 탄창을 가득 채우는 시간(초)입니다. 일부만 보충할 때는 탄 수에 비례해 짧아집니다.")]
        private float _loadTime = 1f;
        [SerializeField]
        private float _maxPitchAngle = 90f;
        [SerializeField]
        private float _minPitchAngle = 0f;
        [SerializeField]
        private float _maxYawAngle = 90f;
        [SerializeField]
        private float _minYawAngle = -90f;

        public int MaxHitCount => _maxHitCount;
        public bool IsInfiniteLoad => _isInfiniteLoad;
        public bool IsAutoLoad => _isAutoLoad;
        public float AimSensitity => _aimSensitity;
        public float MaxYawAngle => _maxYawAngle;
        public float MinYawAngle => _minYawAngle;
        public int LoadMaxCount => _loadMaxCount;
        public float LaunchSpeed => _launchSpeed;
        public BulletData BulletData => _bulletData;
        public float LoadTime => _loadTime;
        public float MaxPitchAngle => _maxPitchAngle;
        public float MinPitchAngle => _minPitchAngle;
    }
}
