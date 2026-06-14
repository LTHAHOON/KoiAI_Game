using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    [SerializeField]
    private CannonBallSkin cannonBallController;
    [SerializeField] 
    private ExplosionTrigger _explosionTrigger;

    public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
    {
        _explosionTrigger.Init(cannonBallData, targetLayerMask);
    }

    public CannonBallSkin GetCannonBallSkin() => cannonBallController;
    public ExplosionTrigger GetExplosionTrigger() => _explosionTrigger;
}
