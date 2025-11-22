using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Config/EfficiencyConfig", fileName = "EfficiencyConfig")]
public class EfficiencyConfigSO : ScriptableObject
{
    [Header("Capacity")]
    [Tooltip("효율 최대 값")]
    public float max = 100f;

    [Header("Drain (per second)")]
    [Tooltip("걷기 중 초당 효율 소모량")]
    public float walkDrainPerSecond = 0f;

    [Tooltip("스프린트 중 초당 효율 소모량")]
    public float sprintDrainPerSecond = 20f;

    [Header("Jump Cost")]
    [Tooltip("점프 1회당 효율 소모량")]
    public float jumpCost = 15f;

    [Header("Regen")]
    [Tooltip("정지 상태에서 초당 효율 회복량")]
    public float idleRegenPerSecond = 15f;

    [Tooltip("이동 중 초당 효율 회복량")]
    public float moveRegenPerSecond = 5f;

    [Tooltip("소모 후 회복 시작까지의 대기 시간")]
    public float regenDelay = 0.6f;

    [Header("Multiplier")]
    [Tooltip("최저 효율에서의 시간 비용 증가 배율")]
    public float maxPenaltyMultiplier = 2.0f;

    [Tooltip("최고 효율에서의 시간 비용 감소 배율")]
    public float bestMultiplier = 1.0f;
}
