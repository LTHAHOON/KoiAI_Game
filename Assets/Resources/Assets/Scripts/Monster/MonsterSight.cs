using UnityEngine;

public class MonsterSight : MonoBehaviour
{
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private float _sightAngle = 60f;

    void Update()
    {
        Vector3 dirToPlayer = _player.position - transform.position;
        dirToPlayer.Normalize();
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
         //   Debug.Log(angle);

        if(angle < _sightAngle)
        {
           // Debug.Log("플레이어 발견");

            Vector3 cross = Vector3.Cross(transform.forward, dirToPlayer);
            if (cross.y > 0)
            {
           //     Debug.Log("플레이어는 오른쪽에 있음");
            }
            else if (cross.y < 0)
            {
                Debug.Log("플레이어는 왼쪽에 있음");
            }
        }
        
    }
}
