using UnityEngine;

public abstract class TBSSkill
{
    protected readonly PlayerController _player;
    protected readonly PlayerStats _stats;
    protected readonly TimeSystemController _timeSystem;

    protected float _cooldown;
    protected float _timeCost;
    protected float _nextAvailableTime = 0f;

    public TBSSkill(PlayerController player,
                    TimeSystemController timeSystem,
                    float cooldown,
                    float timeCost)
    {
        _player = player;
        _stats = player != null ? player.GetComponent<PlayerStats>() : null;
        _timeSystem = timeSystem;

        _cooldown = cooldown;
        _timeCost = timeCost;
    }

    public bool CanUse()
    {
        // 쿨타임 체크
        if (Time.time < _nextAvailableTime)
            return false;

        // 시간 시스템 연결 안 돼 있으면 사용 불가
        if (_timeSystem == null)
        {
            Debug.LogWarning("[TBSSkill] TimeSystemController가 연결되지 않았습니다.");
            return false;
        }

        // 시간 부족
        if (!_timeSystem.HasEnoughTime(_timeCost))
            return false;

        return true;
    }

    public void Use()
    {
        if (!CanUse())
            return;

        _timeSystem.SpendTime(_timeCost);
        _nextAvailableTime = Time.time + _cooldown;

        OnUse();
    }

    // 실제 스킬 행동
    protected abstract void OnUse();
}
