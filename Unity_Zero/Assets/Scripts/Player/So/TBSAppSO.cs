using System.Collections.Generic;
using UnityEngine;

// 앱의 종류 (액티브/패시브)
public enum TBSAppType
{
    Active, // 액티브 스킬 (산데비스탄 등)
    Passive // 패시브 드라이버 (스탯 강화)
}

// 어떤 스탯을 올려줄지 결정하는 열거형
public enum StatType
{
    WalkSpeed,
    SprintCost,
    MaxEfficiency,
    SprintMultiplier,
    JumpHeight,
    TimeDrainRate
    // 필요한 스탯 종류를 여기에 계속 추가하면 됩니다.
}

// 데이터 테이블용 구조체 (어떤 스탯을 + 얼마만큼?)
[System.Serializable]
public struct TBSStatData
{
    public StatType targetStat; // 적용할 스탯 대상
    public float value;         // 적용할 수치
    public StatModType type;    // 연산 타입 (Flat: 더하기 / PercentAdd: 퍼센트)
}

[CreateAssetMenu(menuName = "ProjectZer0/TBS/TBSApp")]
public class TBSAppSO : ScriptableObject
{
    [Header("Basic Info")]
    public string appId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Settings")]
    public TBSAppType appType;
    public int memoryCost; // RAM 사용량

    [Header("Active Skill Settings (Only for Active)")]
    public float baseCooldown;
    public float baseTimeCost;

    [Header("Smart Aim Settings (Only for Smart_Aim)")]
    [Tooltip("Smart_Aim 지속 시간(초)")]
    public float smartAimDuration = 5f;

    [Tooltip("조준 기준 방향과 타겟 사이 최대 허용 각도")]
    public float smartAimMaxLockAngle = 20f;

    [Tooltip("유도 강도 (0~1, 1에 가까울수록 타겟 쪽으로 더 강하게 꺾임)")]
    public float smartAimHomingStrength = 0.7f;

    [Header("Ghost Protocol Settings (Only for Ghost_Protocol)")]
    [Tooltip("투명화 시작 시 초당 시간 소모량")]
    public float ghostBaseCostPerSec = 1f;

    [Tooltip("최대 초당 시간 소모량")]
    public float ghostMaxCostPerSec = 5f;

    [Tooltip("사용 시간이 늘어날 때 초당 시간 소모 증가량")]
    public float ghostGrowthPerSec = 0.3f;

    [Tooltip("이 값 아래로 떨어지면 자동 해제될 최소 잔여 시간")]
    public float ghostMinTimeToKeep = 2f;

    [Tooltip("최대 지속 시간 (0이면 무제한)")]
    public float ghostMaxDuration = 0f;

    [Header("Passive Stats (Only for Passive)")]
    public List<TBSStatData> statBoosts = new List<TBSStatData>();

}