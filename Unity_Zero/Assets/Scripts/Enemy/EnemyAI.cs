using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Tooltip("플레이어 인식 정보를 제공하는 모듈")]
    [SerializeField] private EnemyPerception perception;

    [Tooltip("적 이동용 NavMeshAgent")]
    [SerializeField] private NavMeshAgent agent;

    [Tooltip("플레이어를 시야에서 놓친 뒤 추적을 유지하는 시간")]
    [SerializeField] private float loseSightTime = 2f;

    private float _lastSeenTime = -999f;

    private enum State
    {
        Idle,
        Chase
    }

    private State _state = State.Idle;

    // 로그 스팸 방지용 이전 값
    private State _prevState;
    private bool _prevCanSeePlayer;

    private void Awake()
    {
        // 참조 자동 세팅
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (perception == null)
            perception = GetComponent<EnemyPerception>();

        _prevState = _state;
        _prevCanSeePlayer = false;
    }

    private void Update()
    {
        bool canSeePlayer = perception != null && perception.CanSeePlayer();

        // 상태 변화 또는 인식 결과 변화가 있을 때만 로그 출력
        if (_state != _prevState || canSeePlayer != _prevCanSeePlayer)
        {
            Debug.Log($"[EnemyAI] State={_state}, CanSeePlayer={canSeePlayer}");
            _prevState = _state;
            _prevCanSeePlayer = canSeePlayer;
        }

        switch (_state)
        {
            case State.Idle:
                UpdateIdle(canSeePlayer);
                break;

            case State.Chase:
                UpdateChase(canSeePlayer);
                break;
        }
    }

    private void UpdateIdle(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            _state = State.Chase;
            _lastSeenTime = Time.time;
        }
        else
        {
            if (agent != null && agent.hasPath)
                agent.ResetPath();
        }
    }

    private void UpdateChase(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            _lastSeenTime = Time.time;

            if (agent != null && perception != null && perception.Player != null)
                agent.SetDestination(perception.Player.position);
        }
        else
        {
            if (Time.time - _lastSeenTime > loseSightTime)
            {
                _state = State.Idle;

                if (agent != null && agent.hasPath)
                    agent.ResetPath();
            }
        }
    }
}
