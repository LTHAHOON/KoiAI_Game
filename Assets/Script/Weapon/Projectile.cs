using UnityEngine;

public class Projectile : ResourceItemBase
{
    [SerializeField]
    private Rigidbody _rigid;
    [SerializeField]
    private TrailRenderer _trailRenderer;

    public Rigidbody Rigidbody => _rigid;
    public TrailRenderer TrailRenderer => _trailRenderer;

    public override ItemData GetItemData()
    {
        throw new System.NotImplementedException();
    }
}
