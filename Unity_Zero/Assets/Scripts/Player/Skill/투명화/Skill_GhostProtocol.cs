using UnityEngine;

public class Skill_GhostProtocol : TBSSkill
{
    private readonly PlayerStealthModule _stealthModule;
    private readonly GhostTrailController _ghostTrail;

    private readonly float _baseCostPerSec;
    private readonly float _maxCostPerSec;
    private readonly float _growthPerSec;
    private readonly float _minTimeToKeep;
    private readonly float _maxDuration;

    private bool _isActive;
    private float _elapsed;
    private float _currentCostPerSec;

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

        if (player != null)
            _ghostTrail = player.GetComponentInChildren<GhostTrailController>();
    }

    // 연속 스킬 유지 로직 (시간 소모 계산, 최대 지속시간 체크)
    public void Tick(float deltaTime)
    {
        if (!_isActive)
            return;

        _elapsed += deltaTime;

        // 최대 지속시간 넘으면 강제 종료
        if (_elapsed >= _maxDuration)
        {
            StopSkill();
            return;
        }

        // 초당 소모량 갱신(선형 증가 방식)
        _currentCostPerSec = Mathf.Min(
            _maxCostPerSec,
            _baseCostPerSec + _growthPerSec * _elapsed
        );

        // 실제 시간 소모는 나중에 TimeSystemController 연동 시 추가
        // (지금은 컴파일 안정화 우선)
    }

    // ActiveSkillModule.ExecuteSkill 에서 호출되는 토글 엔트리 포인트
    public void Toggle()
    {
        if (_isActive)
        {
            // 최소 유지시간 전에는 끄기 금지
            if (_elapsed < _minTimeToKeep)
                return;

            StopSkill();
        }
        else
        {
            StartSkill();
        }
    }

    private void StartSkill()
    {
        _isActive = true;
        _elapsed = 0f;
        _currentCostPerSec = _baseCostPerSec;

        if (_stealthModule != null)
            _stealthModule.EnableStealth();

        if (_ghostTrail != null)
            _ghostTrail.SetContinuousTrail(true);
    }

    private void StopSkill()
    {
        _isActive = false;

        if (_stealthModule != null)
            _stealthModule.DisableStealth();

        if (_ghostTrail != null)
            _ghostTrail.SetContinuousTrail(false);
    }

    // TBSSkill 추상 메서드 구현 (이 스킬은 Toggle 기반이라 내부에서 사용 안 함)
    protected override void OnUse()
    {
        // 사용 안 함
    }
}
