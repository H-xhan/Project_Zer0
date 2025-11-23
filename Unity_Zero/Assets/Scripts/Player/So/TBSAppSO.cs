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

    [Header("Passive Stats (Only for Passive)")]
    // [핵심] StatModifier 직접 사용 대신, 데이터 설정용 구조체 리스트 사용
    public List<TBSStatData> statBoosts = new List<TBSStatData>();
}