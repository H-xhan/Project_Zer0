using UnityEngine;

[System.Serializable]
public class StaminaModule
{
    [Header("Capacity")]
    public float max = 100f;
    public float current = 100f;

    [Header("Drain rates per second")]
    public float walkDrainPerSecond = 0f;     // 걷는 중 지속 소모(원하면 0)
    public float sprintDrainPerSecond = 20f;  // 달리기 중 지속 소모

    [Header("Instant costs")]
    public float jumpCost = 15f;              // 점프 1회당 소모

    [Header("Regen")]
    public float idleRegenPerSecond = 15f;    // 가만히 있을 때 회복
    public float moveRegenPerSecond = 5f;     // 걷는 중 회복
    public float regenDelay = 0.6f;           // 소모 후 회복 시작까지 지연

    [Header("Rules")]
    public float minToSprint = 5f;            // 이 값 미만이면 스프린트 불가
    public bool clampOnInit = true;

    private float _regenTimer = 0f;

    public void Init()
    {
        if (clampOnInit) current = Mathf.Clamp(current, 0f, max);
    }

    // ▶ PlayerController 인스펙터 값 동기화
    public void SyncSettings(
        float _minToSprint,
        float _sprintDrainPerSecond,
        float _idleRegenPerSecond,
        float _moveRegenPerSecond,
        float _regenDelay,
        float _jumpCost)
    {
        minToSprint = Mathf.Max(0f, _minToSprint);
        sprintDrainPerSecond = Mathf.Max(0f, _sprintDrainPerSecond);
        idleRegenPerSecond = Mathf.Max(0f, _idleRegenPerSecond);
        moveRegenPerSecond = Mathf.Max(0f, _moveRegenPerSecond);
        regenDelay = Mathf.Max(0f, _regenDelay);
        jumpCost = Mathf.Max(0f, _jumpCost);
    }

    // 매 프레임 호출
    public void Tick(float dt, bool isMoving, bool isSprinting)
    {
        // 지속 소모
        if (isMoving)
        {
            if (isSprinting) Drain(sprintDrainPerSecond * dt);
            else if (walkDrainPerSecond > 0f) Drain(walkDrainPerSecond * dt);
        }

        // 회복(지연 후)
        if (_regenTimer > 0f)
        {
            _regenTimer -= dt;
        }
        else
        {
            float regen = (!isMoving ? idleRegenPerSecond : moveRegenPerSecond) * dt;
            Regen(regen);
        }
    }

    public bool CanSprint() => current >= minToSprint;

    // 즉시 소모(성공 시 true) — 점프/스킬 등에서 사용
    public bool TrySpend(float amount)
    {
        if (amount <= 0f) return true;
        if (current < amount) return false;
        current -= amount;
        _regenTimer = regenDelay; // 소모했으니 회복 딜레이
        return true;
    }

    public void Gain(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Clamp(current + amount, 0f, max);
    }

    public float Normalized() => (max > 0f) ? current / max : 0f;

    void Drain(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Max(0f, current - amount);
        _regenTimer = regenDelay; // 소모 중엔 딜레이 연장
    }

    void Regen(float amount)
    {
        if (amount <= 0f) return;
        current = Mathf.Min(max, current + amount);
    }
}
