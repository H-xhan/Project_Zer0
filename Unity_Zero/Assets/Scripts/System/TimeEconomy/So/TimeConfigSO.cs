using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Config/TimeConfig", fileName = "TimeConfig")]
public class TimeConfigSO : ScriptableObject
{
    [Header("Core")]
    [Tooltip("시작 시 지급할 시간(초)")]
    public float initialSeconds = 900f;

    [Tooltip("초당 기본 소모 시간(루프 진행 등)")]
    public float baseDrainPerSecond = 1f;

    [Tooltip("시작 시 시간 시스템을 자동으로 작동시킬지 여부")]
    public bool startRunning = true;

    [Header("Action Costs")]
    [Tooltip("걷는 동안 초당 차감할 시간(초)")]
    public float walkCostPerSecond = 1f;

    [Tooltip("뛰는 동안 초당 차감할 시간(초)")]
    public float sprintCostPerSecond = 2f;

    [Tooltip("점프 1회당 차감할 시간(초)")]
    public float jumpCostOnce = 3f;

    [Header("Damage Costs")]
    [Tooltip("데미지 1당 차감할 시간(초) 배율")]
    public float damageToSecondsScale = 1f;

    [Tooltip("피해 처리 후 추가 피해를 무시하는 시간(초)")]
    public float damageIFrameSeconds = 0.2f;

    [Header("Dynamic Cost Multiplier")]
    [Tooltip("EfficiencyModule에서 전달받는 시간 비용 배율 기본값")]
    public float externalCostMultiplier = 1f;

    [Tooltip("외부 배율 최소값")]
    public float externalCostMin = 1f;

    [Tooltip("외부 배율 최대값")]
    public float externalCostMax = 5f;

    [Header("Efficiency Influence per Action")]
    [Tooltip("걷기 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float walkEffFactor = 1f;

    [Tooltip("스프린트 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float sprintEffFactor = 1f;

    [Tooltip("점프 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)] public float jumpEffFactor = 1f;

    [Header("Efficiency Penalty Steps")]
    [Tooltip("추가 패널티 1단계가 시작되는 누적 효율 손실량 (% 단위)")]
    [Range(0f, 100f)] public float effStep1LossPercent = 20f;

    [Tooltip("추가 패널티 2단계가 시작되는 누적 효율 손실량 (% 단위)")]
    [Range(0f, 100f)] public float effStep2LossPercent = 40f;

    [Tooltip("추가 패널티 3단계가 시작되는 누적 효율 손실량 (% 단위)")]
    [Range(0f, 100f)] public float effStep3LossPercent = 60f;

    [Tooltip("효율 손실이 0~1단계 구간일 때 시간 비용 배율")]
    public float effMultiplierStage0 = 1.0f;

    [Tooltip("효율 손실이 1~2단계 구간일 때 시간 비용 배율")]
    public float effMultiplierStage1 = 1.5f;

    [Tooltip("효율 손실이 2~3단계 구간일 때 시간 비용 배율")]
    public float effMultiplierStage2 = 2.5f;

    [Tooltip("효율 손실이 3단계를 넘었을 때 시간 비용 배율")]
    public float effMultiplierStage3 = 3.5f;
}
