using UnityEngine;

/// 행동별 시간 비용 계산 전담 모듈
public class TimeActionCostModule : MonoBehaviour
{
    [Header("Base Action Costs (seconds)")]
    [Tooltip("걷는 동안 초당 차감할 시간(초)")]
    [SerializeField] private float walkCostPerSecond = 1f;

    [Tooltip("뛰는 동안 초당 차감할 시간(초)")]
    [SerializeField] private float sprintCostPerSecond = 2f;

    [Tooltip("점프 1회당 차감할 시간(초)")]
    [SerializeField] private float jumpCostOnce = 3f;

    [Header("Efficiency Influence per Action")]
    [Tooltip("걷기 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)]
    [SerializeField] private float walkEffFactor = 1f;

    [Tooltip("스프린트 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)]
    [SerializeField] private float sprintEffFactor = 1f;

    [Tooltip("점프 시간 비용에 효율 배율을 얼마나 반영할지")]
    [Range(0f, 10f)]
    [SerializeField] private float jumpEffFactor = 1f;

    private TimeSystemController _ctrl;

    public void Initialize(TimeSystemController controller)
    {
        _ctrl = controller;
        ApplyConfigFromDataController();
    }

    private void ApplyConfigFromDataController()
    {
        var data = DataController.Instance;
        if (data == null)
            return;

        var cfg = data.TimeConfig;
        if (cfg == null)
            return;

        walkCostPerSecond = cfg.walkCostPerSecond;
        sprintCostPerSecond = cfg.sprintCostPerSecond;
        jumpCostOnce = cfg.jumpCostOnce;

        walkEffFactor = cfg.walkEffFactor;
        sprintEffFactor = cfg.sprintEffFactor;
        jumpEffFactor = cfg.jumpEffFactor;
    }

    public void SpendForWalkDelta(float deltaTime)
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (walkCostPerSecond <= 0f || deltaTime <= 0f)
            return;

        float mul = GetActionCostMultiplier(walkEffFactor);
        float spend = walkCostPerSecond * deltaTime * mul;
        _ctrl.SpendSeconds(spend, "Walk");
    }

    public void SpendForSprintDelta(float deltaTime)
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (sprintCostPerSecond <= 0f || deltaTime <= 0f)
            return;

        float mul = GetActionCostMultiplier(sprintEffFactor);
        float spend = sprintCostPerSecond * deltaTime * mul;
        _ctrl.SpendSeconds(spend, "Sprint");
    }

    public void SpendForJumpEvent()
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (jumpCostOnce <= 0f)
            return;

        float mul = GetActionCostMultiplier(jumpEffFactor);
        float spend = jumpCostOnce * mul;
        _ctrl.SpendSeconds(spend, "Jump");
    }

    private float GetActionCostMultiplier(float effFactor)
    {
        if (_ctrl == null)
            return 1f;

        float external = _ctrl.ExternalCostMultiplier;
        float t = Mathf.Clamp01(effFactor);
        float mul = Mathf.Lerp(1f, external, t);
        return mul;
    }
}
