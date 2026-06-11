using UnityEngine;

public abstract class WeaponControllerBase : MonoBehaviour
{
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
    public abstract void SetAim(Vector3 aim);

    /// <summary>
    /// 스킨 설정하는 함수
    /// </summary>
    public virtual void ChangeSkin() { }
}
