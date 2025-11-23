using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Physical Stats")]
    public Stat WalkSpeed;
    public Stat SprintMultiplier;
    public Stat JumpHeight;

    [Header("Time/Efficiency Stats")]
    public Stat MaxEfficiency;
    public Stat SprintCost;
    public Stat TimeDrainRate;

    private Dictionary<object, List<StatModifier>> _appModifiers = new Dictionary<object, List<StatModifier>>();

    private void Awake()
    {
        WalkSpeed = new Stat(0f);
        SprintMultiplier = new Stat(0f);
        JumpHeight = new Stat(0f);

        MaxEfficiency = new Stat(100f);
        SprintCost = new Stat(0f);
        TimeDrainRate = new Stat(1f);
    }

    private void Start()
    {
        InitializeBaseStats();
    }

    public void InitializeBaseStats()
    {
        var data = DataController.Instance;
        if (data == null) return;

        if (data.MovementConfig != null)
        {
            WalkSpeed.BaseValue = data.MovementConfig.walkSpeed;
            SprintMultiplier.BaseValue = data.MovementConfig.sprintMultiplier;
            JumpHeight.BaseValue = data.MovementConfig.jumpHeight;
        }

        if (data.EfficiencyConfig != null)
        {
            MaxEfficiency.BaseValue = data.EfficiencyConfig.max;
            SprintCost.BaseValue = data.EfficiencyConfig.sprintDrainPerSecond;
        }

        if (data.TimeConfig != null)
        {
            TimeDrainRate.BaseValue = data.TimeConfig.baseDrainPerSecond;
        }

        Debug.Log("[PlayerStats] Base Stats Initialized.");
    }

    public void ApplyAppStats(TBSAppSO app, bool isEquipping)
    {
        if (app == null) return;

        if (isEquipping)
        {
            List<StatModifier> modsToAdd = new List<StatModifier>();

            foreach (var data in app.statBoosts)
            {
                Stat targetStat = GetStatByType(data.targetStat);
                if (targetStat != null)
                {
                    StatModifier mod = new StatModifier(data.value, data.type, app);
                    targetStat.AddModifier(mod);
                    modsToAdd.Add(mod);
                }
            }

            if (modsToAdd.Count > 0)
                _appModifiers[app] = modsToAdd;
        }
        else
        {
            // [수정] GPT 피드백 반영: 불필요한 반복문 제거
            if (_appModifiers.ContainsKey(app))
            {
                // 이 앱(Source)으로 등록된 모든 Modifier를 한 방에 제거
                RemoveModifiersFromAllStats(app);
                _appModifiers.Remove(app);
            }
        }
    }

    private Stat GetStatByType(StatType type)
    {
        switch (type)
        {
            case StatType.WalkSpeed: return WalkSpeed;
            case StatType.SprintCost: return SprintCost;
            case StatType.MaxEfficiency: return MaxEfficiency;
            // [추가] 나머지 스탯들도 연결 (이제 점프력 칩도 만들 수 있음!)
            case StatType.SprintMultiplier: return SprintMultiplier; // *Enum에 추가 필요할 수 있음
            case StatType.JumpHeight: return JumpHeight;             // *Enum에 추가 필요할 수 있음
            case StatType.TimeDrainRate: return TimeDrainRate;       // *Enum에 추가 필요할 수 있음
            default: return null;
        }
    }

    private void RemoveModifiersFromAllStats(object source)
    {
        WalkSpeed.RemoveAllModifiersFromSource(source);
        SprintMultiplier.RemoveAllModifiersFromSource(source);
        JumpHeight.RemoveAllModifiersFromSource(source);
        MaxEfficiency.RemoveAllModifiersFromSource(source);
        SprintCost.RemoveAllModifiersFromSource(source);
        TimeDrainRate.RemoveAllModifiersFromSource(source);
    }
}