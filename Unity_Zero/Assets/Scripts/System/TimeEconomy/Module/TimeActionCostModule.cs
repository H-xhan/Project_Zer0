using UnityEngine;

/// 행동별 추가 시간 차감을 전담하는 모듈
/// - 컨트롤러가 Initialize(this)를 호출해서 연결
/// - 이동 중에는 초당 과금, 점프는 1회 과금
public class TimeActionCostModule : MonoBehaviour
{
    private TimeSystemController ctrl;                 // 허브 참조 저장

    public void Initialize(TimeSystemController controller) // 컨트롤러에서 1회 호출
    {
        ctrl = controller;                              // 허브 주입
    }

    public void SpendForWalkDelta(float deltaTime)
    {
        if (ctrl == null || !ctrl.IsRunning) return;
        if (ctrl.walkCostPerSecond <= 0f || deltaTime <= 0f) return;

        // 기본 걷기 비용 × 걷기용 효율 배율
        float mul = ctrl.GetWalkCostMultiplier();
        float spend = ctrl.walkCostPerSecond * deltaTime * mul;

        ctrl.SpendSeconds(spend, "Walk");
    }

    public void SpendForSprintDelta(float deltaTime)
    {
        if (ctrl == null || !ctrl.IsRunning) return;
        if (ctrl.sprintCostPerSecond <= 0f || deltaTime <= 0f) return;

        // 기본 스프린트 비용 × 스프린트용 효율 배율
        float mul = ctrl.GetSprintCostMultiplier();
        float spend = ctrl.sprintCostPerSecond * deltaTime * mul;

        ctrl.SpendSeconds(spend, "Sprint");
    }
    public void SpendForJumpEvent()
    {
        if (ctrl == null || !ctrl.IsRunning) return;
        if (ctrl.jumpCostOnce <= 0f) return;

        // 기본 점프 비용 × 점프용 효율 배율
        float mul = ctrl.GetJumpCostMultiplier();
        float spend = ctrl.jumpCostOnce * mul;

        ctrl.SpendSeconds(spend, "Jump");
    }
}

