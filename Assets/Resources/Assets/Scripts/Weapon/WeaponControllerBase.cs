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
    
    public abstract void StartAiming(float startPitchAngle, float startYawAngle);
    public abstract void EndAiming();

    /// <summary>
    /// 스킨 설정하는 함수
    /// </summary>

    public abstract void ChangeSkin();

    protected abstract void InitSkin();
    protected bool IsSkinPrefab() => _isSkinPrefab;
    protected Skin GetSkin() => _skin;
}
