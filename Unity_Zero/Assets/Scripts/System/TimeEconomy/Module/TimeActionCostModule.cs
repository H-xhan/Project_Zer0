using UnityEngine;

/// 행동별 시간 비용 계산 전담 모듈
/// TimeSystemController에서 Initialize로 연결된다.
public class TimeActionCostModule : MonoBehaviour
{
    // 시간 시스템 허브 참조
    private TimeSystemController _ctrl;

    // 허브에서 1회 호출
    public void Initialize(TimeSystemController controller)
    {
        _ctrl = controller;
    }

    // 걷기 시간 비용
    public void SpendForWalkDelta(float deltaTime)
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (_ctrl.walkCostPerSecond <= 0f || deltaTime <= 0f)
            return;

        float mul = _ctrl.GetWalkCostMultiplier();
        float spend = _ctrl.walkCostPerSecond * deltaTime * mul;
        _ctrl.SpendSeconds(spend, "Walk");
    }

    // 스프린트 시간 비용
    public void SpendForSprintDelta(float deltaTime)
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (_ctrl.sprintCostPerSecond <= 0f || deltaTime <= 0f)
            return;

        float mul = _ctrl.GetSprintCostMultiplier();
        float spend = _ctrl.sprintCostPerSecond * deltaTime * mul;
        _ctrl.SpendSeconds(spend, "Sprint");
    }

    // 점프 시간 비용
    public void SpendForJumpEvent()
    {
        if (_ctrl == null || !_ctrl.IsRunning)
            return;
        if (_ctrl.jumpCostOnce <= 0f)
            return;

        float mul = _ctrl.GetJumpCostMultiplier();
        float spend = _ctrl.jumpCostOnce * mul;
        _ctrl.SpendSeconds(spend, "Jump");
    }
}
