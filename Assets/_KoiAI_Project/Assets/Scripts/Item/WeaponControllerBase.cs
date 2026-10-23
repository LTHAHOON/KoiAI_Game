using System;
using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Skin;
    using KoiAI.Utilities;
    using R3;

    public abstract class WeaponControllerBase : MonoBehaviour
    {
        [SerializeField]
        private WeaponData _weaponData;
        [SerializeField]
        private bool _isSkinPrefab;
        [SerializeField]
        private Skin _skin;

        protected bool _isAiming = false;

        public event Action<bool> OnReloadStateChanged;

        protected void NotifyReloadStateChanged(bool isReloading)
        {
            OnReloadStateChanged?.Invoke(isReloading);
        }

        public virtual bool IsFireLoading() => false;

        /// <summary>
        /// 장전된 탄을 유지하고 중단합니다. 중단했다면 해당 공격 입력은 소모합니다.
        /// </summary>
        public virtual bool TryInterruptReload() => false;


        /// <summary>
        /// 풀링 또는 값 초기화를 위한 함수
        /// </summary>
        public abstract void Init(WeaponBase wepaonItem);

        /// <summary>
        /// 발동 함수
        /// </summary>
        /// <returns>발동 못할 시 False</returns>
        public abstract bool Activate();

        public virtual bool ActivateWithSpread(float spreadAngle) => Activate();

        protected static Vector3 ApplySpread(Vector3 direction, Vector3 up, float spreadAngle)
        {
            if (spreadAngle <= 0f)
            {
                return direction;
            }

            direction.Normalize();
            Vector3 right = Vector3.Cross(up, direction);
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.Cross(Vector3.right, direction);
            }
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.Cross(Vector3.forward, direction);
            }
            right.Normalize();
            Vector3 spreadUp = Vector3.Cross(direction, right).normalized;
            Vector2 offset = UnityEngine.Random.insideUnitCircle
                * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
            return (direction + right * offset.x + spreadUp * offset.y).normalized;
        }

        /// <summary>
        /// 에임 설정 함수
        /// </summary>
        public abstract void SetAim(Vector2 aim);
    
    
        /// <summary>
        /// 예외) Sword같은 경우 조준선(위치) 설정
        /// </summary>
        public abstract void StartAiming(float startPitchAngle = 0, float startYawAngle = 0);
        public abstract void EndAiming();

        /// <summary>
        /// 스킨 초기 설정하는 함수
        /// </summary>
        protected abstract void InitSkin();
    
        /// <summary>
        /// 스킨 바꾸는 함수
        /// </summary>
        public abstract void ChangeSkin();

        public virtual bool TryGetYawPitch(Vector3 hitPoint, out float yawDeg, out float pitchDeg)
        {
            yawDeg = 0f;
            pitchDeg = 0f;
            return true;
        }

        public bool IsAiming() => _isAiming;
        public WeaponData GetWeaponData() => _weaponData;
        protected T GetWeaponData<T>() where T : WeaponData => (T)_weaponData;
        protected bool IsSkinPrefab() => _isSkinPrefab;
        protected Skin GetSkin() => _skin;
    }
}
