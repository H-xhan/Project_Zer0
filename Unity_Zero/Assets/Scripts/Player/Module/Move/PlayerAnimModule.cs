using UnityEngine;

[System.Serializable]
public class PlayerAnimModule
{
    [Header("Animator")]
    [Tooltip("플레이어 애니메이터")]
    public Animator anim;

    [Tooltip("이동 속도 파라미터 이름 (float)")]
    public string paramSpeed = "Speed";

    [Tooltip("점프 트리거 파라미터 이름")]
    public string paramJump = "Jump";

    [Tooltip("낙하 여부 파라미터 이름 (bool)")]
    public string paramIsFalling = "IsFalling";

    [Tooltip("착지 트리거 파라미터 이름")]
    public string paramLand = "Land";

    [Tooltip("스프린트 여부 파라미터 이름 (bool)")]
    public string paramIsSprinting = "IsSprinting";

    [Header("Speed Param")]
    [Tooltip("실제 속도를 애니메이터 파라미터로 변환하는 스케일 값")]
    public float speedToParam = 0.25f;

    [Tooltip("이 값 이하의 속도는 0으로 스냅")]
    public float stopSnapThreshold = 0.05f;

    [Tooltip("지상일 때만 스냅 처리 적용")]
    public bool snapOnlyWhenGrounded = true;

    [Header("Damping")]
    [Tooltip("속도 증가 시 감쇠 값")]
    public float dampUp = 0.08f;

    [Tooltip("속도 감소 시 감쇠 값")]
    public float dampDown = 0.04f;

    [Header("Falling")]
    [Tooltip("이 값보다 아래 속도면 낙하로 판단")]
    public float fallingThreshold = -0.1f;

    // 내부 상태: 마지막 속도 값, 지상 여부
    float _lastSpeedParam;
    bool _wasGrounded;

    // 초기 애니메이터 설정
    public void Init(Animator a)
    {
        anim = a;
    }

    // PlayerController에서 설정값 동기화 시 사용할 수 있는 메서드
    public void SyncSettings(float dampUpValue, float dampDownValue, float stopSnap)
    {
        dampUp = dampUpValue;
        dampDown = dampDownValue;
        stopSnapThreshold = stopSnap;
    }

    // 매 프레임 애니메이션 업데이트
    public void Tick(
        float dt,
        float planarSpeed,
        bool grounded,
        float verticalVelocity,
        bool jumpTriggered,
        bool isSprinting
    )
    {
        if (!anim)
            return;

        // 이동 속도 파라미터 계산
        float target = Mathf.Max(0f, planarSpeed) * speedToParam;

        if ((!snapOnlyWhenGrounded || grounded) && target <= stopSnapThreshold)
            target = 0f;

        float damp = (target >= _lastSpeedParam) ? dampUp : dampDown;

        if (!string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, target, damp, dt);

        _lastSpeedParam = target;

        // 낙하 상태 플래그
        bool isFalling = !grounded && verticalVelocity < fallingThreshold;
        if (!string.IsNullOrEmpty(paramIsFalling))
            anim.SetBool(paramIsFalling, isFalling);

        // 점프 트리거
        if (jumpTriggered && !string.IsNullOrEmpty(paramJump))
            anim.SetTrigger(paramJump);

        // 착지 트리거
        if (!_wasGrounded && grounded && !string.IsNullOrEmpty(paramLand))
            anim.SetTrigger(paramLand);

        // 스프린트 여부
        if (!string.IsNullOrEmpty(paramIsSprinting))
            anim.SetBool(paramIsSprinting, isSprinting);

        _wasGrounded = grounded;
    }
}
