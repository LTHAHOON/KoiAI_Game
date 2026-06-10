using UnityEngine;

public class MonsterSight : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private float _sightAngle = 60f;
    [SerializeField]
    private float _sightDelayTime = 0.5f;
    
    private float _curSightTime = 0f;
    private bool _isFindPlayer = false;
    void Update()
    {
        SightProcess();
    }

    private void SightProcess()
    {
        if (_curSightTime < _sightDelayTime)
        {
            _curSightTime += Time.deltaTime;
            return;
        }
        Vector3 dirToPlayer = _player.transform.position - transform.position;
        dirToPlayer.Normalize();
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        _isFindPlayer = angle < _sightAngle;
        _curSightTime = 0;
        
        /*
        if(angle < _sightAngle)
        {
            _isFindPlayer = true;

            Vector3 cross = Vector3.Cross(transform.forward, dirToPlayer);
            if (cross.y > 0)
            {
                Debug.Log("플레이어는 오른쪽에 있음");
            }
            else if (cross.y < 0)
            {
                Debug.Log("플레이어는 왼쪽에 있음");
            }
        }
        */
    }
    
    public GameObject GetPlayerToFind()
    {
        return _isFindPlayer ? _player : null;
    }
    
    public bool IsFindPlayer() => _isFindPlayer;


}
