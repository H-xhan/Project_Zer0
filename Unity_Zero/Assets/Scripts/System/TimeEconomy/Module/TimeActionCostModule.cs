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

    public void SpendForWalkDelta(float deltaTime)      // 걷는 중 매 프레임 호출
    {
        if (ctrl == null || !ctrl.IsRunning) return;    // 작동중이 아니면 무시
        if (ctrl.walkCostPerSecond <= 0f || deltaTime <= 0f) return; // 비용/시간 체크

        float spend = ctrl.walkCostPerSecond * deltaTime; // 초당 → 프레임당
        ctrl.SpendSeconds(spend, "Walk");                 // 허브 API로 차감
    }

    public void SpendForSprintDelta(float deltaTime)    // 뛰는 중 매 프레임 호출
    {
        if (ctrl == null || !ctrl.IsRunning) return;
        if (ctrl.sprintCostPerSecond <= 0f || deltaTime <= 0f) return;

        float spend = ctrl.sprintCostPerSecond * deltaTime; // 초당 → 프레임당
        ctrl.SpendSeconds(spend, "Sprint");                 // 허브 API로 차감
    }

    public void SpendForJumpEvent()                      // 점프 성공 시 1회 호출
    {
        if (ctrl == null || !ctrl.IsRunning) return;
        if (ctrl.jumpCostOnce <= 0f) return;

        ctrl.SpendSeconds(ctrl.jumpCostOnce, "Jump");    // 허브 API로 차감
    }
}
