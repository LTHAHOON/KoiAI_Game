using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    [SerializeField]
    private CannonBallSkin _cannonBallSkin;
    [SerializeField] 
    private ExplosionTrigger _explosionTrigger;

    public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
    {
        _explosionTrigger.Init(cannonBallData, targetLayerMask, OnExplosion);
    }

    public void OnExplosion()
    {
        _cannonBallSkin.TrailRenderer.enabled = false;
    }

    public CannonBallSkin GetCannonBallSkin() => _cannonBallSkin;
    public ExplosionTrigger GetExplosionTrigger() => _explosionTrigger;
}
