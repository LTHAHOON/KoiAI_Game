using UnityEngine;

public class BulletData : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rigid;
    [SerializeField]
    private TrailRenderer _trailRenderer;

    public Rigidbody Rigidbody => _rigid;
    public TrailRenderer TrailRenderer => _trailRenderer;
}
