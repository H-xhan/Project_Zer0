using UnityEngine;

[System.Serializable]
public class EfficiencyModule
{
    [Header("Capacity")]
    [Tooltip("효율 최대 값")]
    public float max = 100f;

    [Tooltip("현재 효율 값")]
    public float current = 100f;

    [Header("Drain Rates (per second)")]
    [Tooltip("걷는 중 초당 효율 소모량 (0이면 소모 없음)")]
    public float walkDrainPerSecond = 0f;

    [Tooltip("스프린트 중 초당 효율 소모량")]
    public float sprintDrainPerSecond = 20f;

    [Header("Instant Costs")]
    [Tooltip("점프 1회당 효율 소모량")]
    public float jumpCost = 15f;

    [Header("Regen")]
    [Tooltip("정지 상태 초당 회복량")]
    public float idleRegenPerSecond = 15f;

    [Tooltip("이동 중 초당 회복량")]
    public float moveRegenPerSecond = 5f;

    [Tooltip("소모 후 회복 시작까지 지연 시간")]
    public float regenDelay = 0.6f;

    [Header("Init")]
    [Tooltip("Init 시 현재 값을 0~max 범위로 보정할지 여부")]
    public bool clampOnInit = true;

    [Header("Efficiency → Cost Multiplier")]
    [Tooltip("효율이 가장 낮을 때의 비용 배율")]
    public float maxPenaltyMultiplier = 2.0f;

    [Tooltip("효율이 최대일 때의 비용 배율")]
    public float bestMultiplier = 1.0f;

    // 회복 대기 타이머
    float _regenTimer;

    // 초기화: 시작 값 보정
    public void Init()
    {
        if (clampOnInit)
            current = Mathf.Clamp(current, 0f, max);
    }

    // PlayerController에서 인스펙터 값 동기화
    public void SyncSettings(
        float walkDrain,
        float sprintDrain,
        float idleRegen,
        float moveRegen,
        float delay,
        float jumpCostValue,
        float maxCapacity
    )
    {
        max = Mathf.Max(1f, maxCapacity);

        walkDrainPerSecond = Mathf.Max(0f, walkDrain);
        sprintDrainPerSecond = Mathf.Max(0f, sprintDrain);
        idleRegenPerSecond = Mathf.Max(0f, idleRegen);
        moveRegenPerSecond = Mathf.Max(0f, moveRegen);
        regenDelay = Mathf.Max(0f, delay);
        jumpCost = Mathf.Max(0f, jumpCostValue);

        current = Mathf.Clamp(current, 0f, max);
    }

    // 매 프레임 효율 변화 처리
    public void Tick(float dt, bool isMoving, bool isSprinting)
    {
        // 지속 소모
        if (isMoving)
        {
            if (isSprinting)
            {
                Drain(sprintDrainPerSecond * dt);
            }
            else if (walkDrainPerSecond > 0f)
            {
                Drain(walkDrainPerSecond * dt);
            }
        }

        // 회복 지연 처리
        if (_regenTimer > 0f)
        {
            _regenTimer -= dt;
            return;
        }

        // 이동 여부에 따라 회복량 선택
        float regen = (isMoving ? moveRegenPerSecond : idleRegenPerSecond) * dt;
        Regen(regen);
    }

    // 행동 시 즉시 소모 (성공 여부 반환하지만 현재는 항상 true)
    public bool TrySpend(float amount)
    {
        if (amount <= 0f)
            return true;

        current = Mathf.Max(0f, current - amount);
        _regenTimer = regenDelay;
        return true;
    }

    // 외부에서 효율 회복 시 호출
    public void Gain(float amount)
    {
        if (amount <= 0f)
            return;

        current = Mathf.Clamp(current + amount, 0f, max);
    }

    // 0~1 정규화 값 반환
    public float Normalized()
    {
        if (max <= 0f)
            return 0f;

        return current / max;
    }

    // 효율에 따라 시간 소비 배율 계산
    public float ComputeCostMultiplier()
    {
        float t = Normalized();          // 0~1, 높을수록 효율 좋음
        float s = 1f - t;                // 효율이 낮을수록 s 증가
        float curve = s * s;             // 낮은 구간에서 더 급격히 증가

        return Mathf.Lerp(bestMultiplier, maxPenaltyMultiplier, curve);
    }

    // 내부 소모 처리
    void Drain(float amount)
    {
        if (amount <= 0f)
            return;

        current = Mathf.Max(0f, current - amount);
        _regenTimer = regenDelay;
    }

    // 내부 회복 처리
    void Regen(float amount)
    {
        if (amount <= 0f)
            return;

        current = Mathf.Min(max, current + amount);
    }
}
