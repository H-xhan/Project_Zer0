using UnityEngine;
using System;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;                  // 최대 스태미나
    public float regenPerSec = 15f;                  // 초당 회복량
    public float regenDelay = 0.5f;                  // 소모 후 회복 지연(초)
    public float sprintDrainPerSec = 20f;            // 스프린트 초당 소모량
    public float walkDrainPerSec = 5f;               //걷기 초당 소모(달리기보다 적게)
    public float jumpCost = 15f;                     // 점프 1회 소모량
    public float minSprintToStart = 10f;             // 스프린트 시작 최소치(이보다 낮으면 시작 불가)

    [Header("Debug")]
    public float current;                            // 현재 스태미나(인스펙터 확인용)
    float regenTimer = 0f;                           // 회복 지연 타이머
    bool draining = false;                           // 이번 프레임에 소모 중인지

    public event Action<float, float> OnStaminaChanged; // (current, max) UI 업데이트용 이벤트



    void Awake()
    {
        current = maxStamina;                        // 시작은 풀스태미나
        RaiseChanged();
    }

    void Update()
    {
        // 회복 지연 타이머
        if (regenTimer > 0f) regenTimer -= Time.deltaTime;

        // 소모 중이 아니고 지연 끝났으면 회복
        if (!draining && regenTimer <= 0f && current < maxStamina)
        {
            current = Mathf.Min(maxStamina, current + regenPerSec * Time.deltaTime); // 서서히 회복
            RaiseChanged();
        }

        // 다음 프레임을 위해 드레이닝 플래그 리셋
        draining = false;
    }

    void RaiseChanged() => OnStaminaChanged?.Invoke(current, maxStamina);

    // 외부에서 “스프린트 중”일 때 매 프레임 호출해서 깎아주는 함수
    public bool DrainSprintTick()
    {
        float cost = sprintDrainPerSec * Time.deltaTime;       // 프레임당 비용
        if (current <= 0f) return false;                       // 0이면 불가
        current = Mathf.Max(0f, current - cost);               // 소모
        draining = true;                                       // 이번 프레임 소모중
        regenTimer = regenDelay;                               // 회복 딜레이 갱신
        RaiseChanged();
        return current > 0f;                                   // 남아있으면 true
    }

    // 점프같은 즉시 소모
    public bool TrySpend(float amount)
    {
        if (current < amount) return false;                    // 부족
        current -= amount;                                     // 소모
        draining = true;                                       // 이번 프레임 소모중
        regenTimer = regenDelay;                               // 회복 딜레이
        RaiseChanged();
        return true;
    }

    // 스프린트 시작 가능 여부(최소치 이상)
    public bool CanStartSprint() => current >= minSprintToStart;

    // 현재 비율(0~1)
    public float Ratio() => (maxStamina <= 0f) ? 0f : current / maxStamina;

    public bool DrainWalkTick()
    {
        float cost = walkDrainPerSec * Time.deltaTime;   // 프레임당 소모량
        if (current <= 0f) return false;                 // 0이면 더 깎지 않음
        current = Mathf.Max(0f, current - cost);         // 소모
        draining = true;                                 // 이번 프레임은 소모 중
        regenTimer = regenDelay;                         // 회복 지연 리셋
        RaiseChanged();
        return current > 0f;
    }

    public void Recover(float amount) // 소비아이템/이벤트로 즉시 회복
    {
        if (amount <= 0f) return;

    }
}
