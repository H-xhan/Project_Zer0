using UnityEngine;

public class Skill_SmartAim : TBSSkill
{
    private SmartAimController _smartAim;

    private float _duration;
    private float _maxLockAngle;
    private float _homingStrength;

    public Skill_SmartAim(PlayerController player,
                          TimeSystemController timeSystem,
                          float cooldown,
                          float timeCost,
                          float damageMultiplier,
                          float duration,
                          float maxLockAngle,
                          float homingStrength)
        : base(player, timeSystem, cooldown, timeCost, damageMultiplier)
    {
        if (player != null)
            _smartAim = player.GetComponentInChildren<SmartAimController>();

        _duration = duration;
        _maxLockAngle = maxLockAngle;
        _homingStrength = homingStrength;
    }

    protected override void OnUse()
    {
        if (_smartAim == null)
        {
            Debug.LogWarning("[Skill_SmartAim] SmartAimController 가 플레이어에 없습니다.");
            return;
        }

        _smartAim.Activate(_duration, _maxLockAngle, _homingStrength);
        Debug.Log("[Skill_SmartAim] Smart Aim 활성화");
    }
}
