using UnityEngine;

public class Projectile : ResourceBase
{
    [SerializeField]
    private Rigidbody _rigid;
    [SerializeField]
    private TrailRenderer _trailRenderer;
    
    public Rigidbody Rigidbody => _rigid;
    public TrailRenderer TrailRenderer => _trailRenderer;
}
