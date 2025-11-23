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

        // 남은 값에서 amount만큼 깎되, 최소 0까지만
        current = Mathf.Max(0f, current - amount);
        _regenTimer = regenDelay;
        return true; // 항상 true 반환 → 행동 차단 없음
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

    // 효율에 따라 시간 소비 배율 계산 (계단식 패널티)
    // 효율에 따라 시간 소비 배율 계산 (TimeConfig 기반 계단식 패널티)
    public float ComputeCostMultiplier()
    {
        if (max <= 0f)
            return 1f;

        // 0~1 정규화된 효율 (1 = 100%)
        float efficiency01 = Mathf.Clamp01(current / max);

        // 잃어버린 효율량을 %로 환산 (0~100)
        float lostPercent = (1f - efficiency01) * 100f;

        // 기본값(fallback) – TimeConfigSO가 없을 때 사용
        float step1 = 20f;
        float step2 = 40f;
        float step3 = 60f;

        float mul0 = 1.0f;
        float mul1 = 1.5f;
        float mul2 = 2.5f;
        float mul3 = 3.5f;

        // TimeConfigSO에서 값 가져오기
        var data = DataController.Instance;
        var cfg = data != null ? data.TimeConfig : null;

        if (cfg != null)
        {
            step1 = Mathf.Clamp(cfg.effStep1LossPercent, 0f, 100f);
            step2 = Mathf.Clamp(cfg.effStep2LossPercent, 0f, 100f);
            step3 = Mathf.Clamp(cfg.effStep3LossPercent, 0f, 100f);

            mul0 = cfg.effMultiplierStage0;
            mul1 = cfg.effMultiplierStage1;
            mul2 = cfg.effMultiplierStage2;
            mul3 = cfg.effMultiplierStage3;
        }

        // 계단식 배율 적용
        if (lostPercent < step1)
            return mul0;
        if (lostPercent < step2)
            return mul1;
        if (lostPercent < step3)
            return mul2;
        return mul3;
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