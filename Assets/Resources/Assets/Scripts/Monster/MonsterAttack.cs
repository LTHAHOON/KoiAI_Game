using UnityEngine;

public class MonsterAttack : MonsterFeature
{
    [Header("몬스터 무기")]
    [SerializeField]
    private WeaponControllerBase[] _weaponController;

    public override MonsterState State => MonsterState.Attack;

    public override void EnterFeature()
    {

    }

    public override void UpdateFeature()
    {

    }

    public override void ExitFeature()
    {
    }
}
