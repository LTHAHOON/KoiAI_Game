using UnityEngine;

public class CannonBallSkin : ProjectileSkin
{
    [SerializeField]
    private Rigidbody _rigid;
    [SerializeField]
    private TrailRenderer _trailRenderer;

    public Rigidbody Rigidbody => _rigid;
    public TrailRenderer TrailRenderer => _trailRenderer;
}
