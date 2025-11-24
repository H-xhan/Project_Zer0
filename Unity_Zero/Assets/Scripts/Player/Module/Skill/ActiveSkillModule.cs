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

        // 디버그용: 현재 appId 확인
        string id = app.appId != null ? app.appId.Trim() : string.Empty;
        Debug.Log($"[ActiveSkillModule] 스킬 생성 시도: id = {id}");

        // [임시 정책]
        //  - 일단 모든 Active 앱은 Skill_Rewind로 생성
        //  - 나중에 ID / 타입별로 분기 추가 예정
        return new Skill_Rewind(
            _player,
            _timeSystem,
            app.baseCooldown,
            app.baseTimeCost
        );
    }

    // TBSDeviceController.Update()에서 매 프레임 호출
    public void Tick(float deltaTime)
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i] is Skill_Rewind rewind)
                rewind.Tick();
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

        // 슬롯에 스킬이 없는 경우 (앱이 없거나, Passive 앱이거나, 미구현 앱)
        if (skill == null)
        {
            var apps = _device?.EquippedApps;
            string appId = "null";

            if (apps != null && slotIndex < apps.Count && apps[slotIndex] != null)
                appId = apps[slotIndex].appId;

            Debug.LogWarning($"[ActiveSkillModule] 슬롯 {slotIndex} (앱 {appId}) 에 연결된 스킬이 없습니다.");
            return;
        }

        skill.Use();
    }
}
