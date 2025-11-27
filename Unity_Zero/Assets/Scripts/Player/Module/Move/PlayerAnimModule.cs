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

    [Header("Turn")]
    [Tooltip("턴 파라미터 이름 (float)")]
    public string paramTurn = "Turn";

    [Tooltip("턴 여부 파라미터 이름 (bool)")]
    public string paramIsTurning = "IsTurning";

    [Tooltip("턴으로 판단할 최소 입력값 절대값")]
    public float turnThreshold = 0.25f;

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

    float _lastSpeedParam;
    bool _wasGrounded;

    public void Init(Animator a)
    {
        anim = a;
    }

    public void SyncSettings(float dampUpValue, float dampDownValue, float stopSnap)
    {
        dampUp = dampUpValue;
        dampDown = dampDownValue;
        stopSnapThreshold = stopSnap;
    }

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

        // [수정] 복잡한 계산식 대신 직관적으로 변경
        // 걷기 속도가 약 4, 달리기 속도가 약 7이라고 가정할 때
        // 블렌드 트리가 0(Idle), 0.5(Walk), 1(Run) 이라면:

        float target = 0f;
        if (planarSpeed > 0.1f)
        {
            // 달리는 중이면 1.0, 걷는 중이면 0.5 근처가 되도록 유도
            target = isSprinting ? 1.0f : 0.5f;
        }

        // 부드럽게 보간 (Damping)
        float damp = (target >= _lastSpeedParam) ? dampUp : dampDown;

        if (!string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, target, damp, dt);

        _lastSpeedParam = target;

        // ... (나머지 코드는 그대로)

        // Falling
        bool isFalling = !grounded && verticalVelocity < fallingThreshold;
        if (!string.IsNullOrEmpty(paramIsFalling))
            anim.SetBool(paramIsFalling, isFalling);

        // Jump
        if (jumpTriggered && !string.IsNullOrEmpty(paramJump))
            anim.SetTrigger(paramJump);

        // Land
        if (!_wasGrounded && grounded && !string.IsNullOrEmpty(paramLand))
            anim.SetTrigger(paramLand);

        // Sprint
        if (!string.IsNullOrEmpty(paramIsSprinting))
            anim.SetBool(paramIsSprinting, isSprinting);

        _wasGrounded = grounded;
    }

    private float _turnSmooth; // 내부 보간용

    // 회전 입력 기반 Turn 애니메이션 업데이트 

    public void UpdateTurn(float rawTurnInput, float planarSpeed, bool grounded)
    {
        if (!anim)
            return;

        if (string.IsNullOrEmpty(paramTurn) || string.IsNullOrEmpty(paramIsTurning))
            return;

        // 마우스 입력 안정화 (감도 조절)
        float targetTurn = Mathf.Clamp(rawTurnInput * 0.7f, -1f, 1f);
        if (Mathf.Abs(targetTurn) < 0.05f)
            targetTurn = 0f;

        // 부드럽게 보간 (회전 전환 완화)
        _turnSmooth = Mathf.Lerp(_turnSmooth, targetTurn, Time.deltaTime * 6f);

        // 제자리 상태에서만 턴 활성화
        bool isIdle = planarSpeed < 0.1f;
        bool isTurning = grounded && isIdle && Mathf.Abs(_turnSmooth) > turnThreshold;

        anim.SetFloat(paramTurn, _turnSmooth);
        anim.SetBool(paramIsTurning, isTurning);
    }
}
