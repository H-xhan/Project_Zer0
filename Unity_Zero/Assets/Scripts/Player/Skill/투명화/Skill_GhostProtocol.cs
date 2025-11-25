using UnityEngine;

public class Skill_GhostProtocol : TBSSkill
{
    private PlayerStealthModule _stealthModule;

    private bool _isActive;
    private float _elapsed;

    private readonly float _baseCostPerSec;
    private readonly float _maxCostPerSec;
    private readonly float _growthPerSec;
    private readonly float _minTimeToKeep;
    private readonly float _maxDuration;

    public Skill_GhostProtocol(
        PlayerController player,
        TimeSystemController timeSystem,
        float cooldown,
        float timeCost,
        float damageMultiplier,
        PlayerStealthModule stealthModule,
        float baseCostPerSec,
        float maxCostPerSec,
        float growthPerSec,
        float minTimeToKeep,
        float maxDuration
    ) : base(player, timeSystem, cooldown, timeCost, damageMultiplier)
    {
        _stealthModule = stealthModule;
        _baseCostPerSec = baseCostPerSec;
        _maxCostPerSec = maxCostPerSec;
        _growthPerSec = growthPerSec;
        _minTimeToKeep = minTimeToKeep;
        _maxDuration = maxDuration;
    }

    // Player 자식 오브젝트(Stealth)에 붙어 있는 모듈까지 포함해서 찾아오는 헬퍼
    private void EnsureStealthModule()
    {
        if (_stealthModule != null)
            return;

        if (_player != null)
        {
            _stealthModule = _player.GetComponentInChildren<PlayerStealthModule>();
            if (_stealthModule == null)
            {
                Debug.LogWarning("[Skill_GhostProtocol] PlayerStealthModule을 찾을 수 없습니다.");
            }
        }
    }

    // R 키로 호출할 메서드 (ActiveSkillModule에서 사용)
    public void Toggle()
    {
        // 켜져 있으면 비용/쿨 없이 끄기
        if (_isActive)
        {
            Deactivate();
            return;
        }

        // 꺼져있을 때 켜는 경우만 기본 CanUse/쿨타임/초기 비용 적용
        if (!CanUse())
            return;

        if (_timeSystem == null)
            return;

        // TBSSkill.Use() 로직을 그대로 수동 적용
        _timeSystem.SpendTime(_timeCost);
        _nextAvailableTime = Time.time + _cooldown;

        Activate();
    }

    protected override void OnUse()
    {
        // 이 스킬은 Use()를 직접 호출하지 않고 Toggle()만 사용
    }

    private void Activate()
    {
        _isActive = true;
        _elapsed = 0f;

        EnsureStealthModule();
        if (_stealthModule != null)
            _stealthModule.SetInvisible(true);

        Debug.Log("[Skill_GhostProtocol] 활성화");
    }

    private void Deactivate()
    {
        _isActive = false;
        _elapsed = 0f;

        EnsureStealthModule();
        if (_stealthModule != null)
            _stealthModule.SetInvisible(false);

        Debug.Log("[Skill_GhostProtocol] 비활성화");
    }

    // 매 프레임 ActiveSkillModule.Tick()에서 호출
    public void Tick(float deltaTime)
    {
        if (!_isActive)
            return;

        if (_timeSystem == null)
        {
            Deactivate();
            return;
        }

        _elapsed += deltaTime;

        // 최대 지속 시간
        if (_maxDuration > 0f && _elapsed >= _maxDuration)
        {
            Debug.Log("[Skill_GhostProtocol] 최대 지속 시간 도달");
            Deactivate();
            return;
        }

        // 남은 시간이 너무 적으면 자동 해제
        if (!_timeSystem.HasEnoughTime(_minTimeToKeep))
        {
            Debug.Log("[Skill_GhostProtocol] 잔여 시간 부족으로 자동 해제");
            Deactivate();
            return;
        }

        // 시간 소모량 계산 (선형 증가)
        float costPerSec = _baseCostPerSec + _growthPerSec * _elapsed;
        if (costPerSec > _maxCostPerSec)
            costPerSec = _maxCostPerSec;

        float costThisFrame = costPerSec * deltaTime;
        _timeSystem.SpendTime(costThisFrame);
    }
}
