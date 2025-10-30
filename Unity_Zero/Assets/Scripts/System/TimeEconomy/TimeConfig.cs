using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Time Economy/TimeConfig")]
public class TimeConfig : ScriptableObject
{
    [Header("Base Loop")]
    public int initialSeconds = 900;            // 시작 생존 시간(초)
    public float baseDrainPerSecond = 1f;       // 기본 초당 감산

    [Header("Action Costs (fallback)")]
    public float moveCostPerMeter = 0.1f;       // 이동 1m당
    public float jumpCost = 0.5f;               // 점프 1회
    public float sprintExtraPerSecond = 0.2f;   // 스프린트 중 초당

    [Header("Taxes & Upkeep")]
    public float loopTaxRate = 0.10f;           // 루프 종료 시 % 과세
    public float periodicUpkeepSeconds = 30f;   // 유지비 주기(초)
    public float upkeepFlatCost = 2f;           // 주기 고정 비용(초)

    [Header("Zones")]
    public float defaultZoneMultiplier = 1f;    // 구역 배율

    [Header("Debug")]
    public bool logTransactions = true;
}
