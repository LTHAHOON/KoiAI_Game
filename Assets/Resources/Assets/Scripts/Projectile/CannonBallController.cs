using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    [SerializeField]
    private CannonBallSkin _cannonBallSkin;
    [SerializeField] 
    private ExplosionTrigger _explosionTrigger;
    [SerializeField]
    private GameObject _explosionPrefab;

    public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
    {
        _explosionTrigger.Init(cannonBallData, targetLayerMask, OnExplosion);
    }

    public void OnExplosion(Vector3 hitPoint)
    {
        _cannonBallSkin.TrailRenderer.enabled = false;
        if(_explosionPrefab)
        {
            Instantiate(_explosionPrefab, hitPoint, Quaternion.identity);
        }
    }

    public CannonBallSkin GetCannonBallSkin() => _cannonBallSkin;
    public ExplosionTrigger GetExplosionTrigger() => _explosionTrigger;
}
