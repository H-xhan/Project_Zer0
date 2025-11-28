using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Tooltip("인식 대상 플레이어 트랜스폼")]
    [SerializeField] private Transform player;

    [Tooltip("플레이어 스텔스 모듈")]
    [SerializeField] private PlayerStealthModule playerStealth;

    [Tooltip("시야 거리")]
    [SerializeField] private float viewDistance = 12f;

    private void Awake()
    {
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (playerStealth == null && player != null)
            playerStealth = player.GetComponent<PlayerStealthModule>();
    }

    public bool CanSeePlayer()
    {
        if (player == null)
            return false;

        // 스텔스면 인식 불가
        bool isStealthed = playerStealth != null && playerStealth.IsStealthed;
        if (isStealthed)
            return false;

        // 거리로만 체크 (현재 버전)
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        return sqrDist <= viewDistance * viewDistance;
    }

    public Transform Player => player;
}
