using UnityEngine;

public abstract class WeaponControllerBase : MonoBehaviour
{
    [SerializeField]
    private bool _isSkinPrefab;
    [SerializeField]
    private Skin _skin;
    /// <summary>
    /// 풀링 또는 값 초기화를 위한 함수
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 발동 함수
    /// </summary>
    /// <returns>발동 못할 시 False</returns>
    public abstract bool Activate();

    /// <summary>
    /// 에임 설정 함수
    /// </summary>
    public abstract void SetAim(Vector2 aim);
    
    
    /// <summary>
    /// 예외) Sword같은 경우 조준선(위치) 설정
    /// </summary>
    public abstract void StartAiming(float startPitchAngle, float startYawAngle);
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

    protected bool IsSkinPrefab() => _isSkinPrefab;
    protected Skin GetSkin() => _skin;
}
