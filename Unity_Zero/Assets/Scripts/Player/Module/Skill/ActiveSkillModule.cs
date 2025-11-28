using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveSkillModule
{
    private PlayerController _player;
    private TBSDeviceController _device;
    private TimeSystemController _timeSystem;

    // 슬롯 인덱스와 1:1로 맞춰서 사용하는 스킬 리스트
    // (_device.EquippedApps[i] ↔ _skills[i])
    private readonly List<TBSSkill> _skills = new List<TBSSkill>();

    public void Initialize(PlayerController player,
                           TBSDeviceController device,
                           TimeSystemController timeSystem)
    {
        _player = player;
        _device = device;
        _timeSystem = timeSystem;

        RefreshSkills();   // ← 여기서 RefreshSkills 호출
    }

    // 장착된 앱을 기준으로 슬롯별 스킬 다시 구성
    public void RefreshSkills()
    {
        _skills.Clear();

        if (_device == null) return;
        var apps = _device.EquippedApps;
        if (apps == null) return;

        for (int i = 0; i < apps.Count; i++)
        {
            TBSAppSO app = apps[i];

            // 슬롯 개수 맞추기 위해 무조건 하나씩 push
            if (app == null)
            {
                _skills.Add(null);
                continue;
            }

            if (app.appType != TBSAppType.Active)
            {
                // 액티브 앱이 아니면 스킬 없음
                _skills.Add(null);
                continue;
            }

            TBSSkill skill = CreateSkillFromApp(app);
            _skills.Add(skill);
        }
    }

    private TBSSkill CreateSkillFromApp(TBSAppSO app)
    {
        if (_player == null || _timeSystem == null || app == null)
            return null;

        float cooldown = app.baseCooldown;
        float timeCost = app.baseTimeCost;
        float damageMul = 1f;

        if (_device != null)
        {
            cooldown *= _device.GetCpuCooldownMultiplier();
            timeCost *= _device.GetCpuTimeCostMultiplier();
            damageMul = _device.GetBatteryDamageMultiplier();
        }

        switch (app.appId)
        {
            case "Smart_Aim":
                return new Skill_SmartAim(
                    _player,
                    _timeSystem,
                    cooldown,
                    timeCost,
                    damageMul,
                    app.smartAimDuration,
                    app.smartAimMaxLockAngle,
                    app.smartAimHomingStrength
                );

            case "Ghost_Protocol":
                {
                    var stealthModule = _player.GetComponentInChildren<PlayerStealthModule>();

                    if (stealthModule == null)
                        Debug.LogWarning("[ActiveSkillModule] Ghost_Protocol 사용을 위해 PlayerStealthModule이 필요합니다.");

                    return new Skill_GhostProtocol(
                        _player,
                        _timeSystem,
                        cooldown,
                        timeCost,
                        damageMul,
                        stealthModule,
                        app.ghostBaseCostPerSec,
                        app.ghostMaxCostPerSec,
                        app.ghostGrowthPerSec
                    );
                }

            case "Rewind":
            default:
                return new Skill_Rewind(
                    _player,
                    _timeSystem,
                    cooldown,
                    timeCost,
                    damageMul
                );
        }
    }



    // TBSDeviceController.Update()에서 매 프레임 호출
    public void Tick(float deltaTime)
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i] is Skill_Rewind rewind)
            {
                rewind.Tick();
            }
            else if (_skills[i] is Skill_GhostProtocol ghost)
            {
                ghost.Tick(deltaTime);
            }
        }
    }

    // TBSDeviceController.UseQuickSlot(slotIndex)에서 호출
    public void ExecuteSkill(int slotIndex)
    {
        if (_skills.Count == 0)
        {
            Debug.LogWarning("[ActiveSkillModule] 스킬 리스트가 비어 있습니다. RefreshSkills가 호출되었는지 확인하세요.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= _skills.Count)
        {
            Debug.LogWarning($"[ActiveSkillModule] 잘못된 슬롯 인덱스 {slotIndex}.");
            return;
        }

        TBSSkill skill = _skills[slotIndex];

        if (skill == null)
        {
            var apps = _device?.EquippedApps;
            string appId = "null";

            if (apps != null && slotIndex < apps.Count && apps[slotIndex] != null)
                appId = apps[slotIndex].appId;

            Debug.LogWarning($"[ActiveSkillModule] 슬롯 {slotIndex} (앱 {appId}) 에 연결된 스킬이 없습니다.");
            return;
        }

        // Ghost_Protocol은 토글형
        if (skill is Skill_GhostProtocol ghostSkill)
        {
            ghostSkill.Toggle();
            return;
        }

        // 나머지는 기존대로 단발 Use()
        skill.Use();
    }
}
