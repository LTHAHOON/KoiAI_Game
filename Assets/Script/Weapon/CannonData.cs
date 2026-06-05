using UnityEngine;

[CreateAssetMenu(fileName = "new CannonData", menuName = "WeaponData/CannonData")]
public class CannonData : WeaponData
{
    [SerializeField]
    private CannonBallData _cannonBallData;

    public CannonBallData CannonBallData => _cannonBallData;
}
