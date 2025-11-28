using UnityEngine;

public class Skill_GhostProtocol : TBSSkill
{
    private readonly PlayerStealthModule _stealthModule;

    // 시간 소모 관련 기본값 (지금은 계산만 하고 자동 종료는 안 씀)
    private readonly float _baseCostPerSec;
    private readonly float _maxCostPerSec;
    private readonly float _growthPerSec;

    private bool _isActive;
    private float _elapsed;
    private float _currentCostPerSec;

    public bool IsActive => _isActive;

    public Skill_GhostProtocol(
        PlayerController player,
        TimeSystemController timeSystem,
        float cooldown,
        float timeCost,
        float damageMultiplier,
        PlayerStealthModule stealthModule,
        float baseCostPerSec,
        float maxCostPerSec,
        float growthPerSec
    ) : base(player, timeSystem, cooldown, timeCost, damageMultiplier)
    {
        _stealthModule = stealthModule;
        _baseCostPerSec = baseCostPerSec;
        _maxCostPerSec = maxCostPerSec;
        _growthPerSec = growthPerSec;
    }

    /// <summary>
    /// 매 프레임 ActiveSkillModule에서 호출되는 틱 함수
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!_isActive)
            return;

        _elapsed += deltaTime;

        // 시간 소모량만 계산 (나중에 TimeSystemController에 연동)
        _currentCostPerSec = Mathf.Min(
            _maxCostPerSec,
            _baseCostPerSec + _growthPerSec * _elapsed
        );
    }

    /// <summary>
    /// R 키 눌렸을 때 호출되는 토글 함수
    /// </summary>
    public void Toggle()
    {
        if (_isActive)
        {
            StopSkill();
        }
        else
        {
            StartSkill();
        }
    }

    private void StartSkill()
    {
        if (_isActive)
            return;

        Debug.Log("[GhostProtocol] StartSkill 호출");

        _isActive = true;
        _elapsed = 0f;
        _currentCostPerSec = _baseCostPerSec;

        if (_stealthModule != null)
            _stealthModule.EnableStealth();
        else
            Debug.LogWarning("[GhostProtocol] PlayerStealthModule이 연결되지 않았습니다.");
    }

    private void StopSkill()
    {
        if (!_isActive)
            return;

        Debug.Log("[GhostProtocol] StopSkill 호출");

        _isActive = false;

        if (_stealthModule != null)
            _stealthModule.DisableStealth();
    }

    // TBSSkill 추상 메서드 구현 (이 스킬은 Toggle 기반)
    protected override void OnUse()
    {
        // 사용 안 함 – ActiveSkillModule에서 Toggle()만 호출
    }
}
