using UnityEngine;

[System.Serializable]
public class PlayerAnimModule
{
    [Header("Animator")]
    public Animator anim;
    public string paramSpeed = "Speed";             // float
    public string paramJump = "Jump";               // trigger
    public string paramIsFalling = "IsFalling";     // bool
    public string paramLand = "Land";               // trigger
    public string paramIsSprinting = "IsSprinting"; // bool

    [Header("Speed → Param")]
    public float speedToParam = 0.25f;              // planarSpeed * scale
    public float stopSnapThreshold = 0.05f;         // 이 값 이하이면 즉시 0으로 스냅
    public bool snapOnlyWhenGrounded = true;        // 공중에서는 스냅 안 함

    [Header("Damping")]
    public float dampUp = 0.08f;                    // 가속 시 감쇠
    public float dampDown = 0.04f;                  // 감속 시 감쇠

    [Header("Falling")]
    public float fallingThreshold = -0.1f;

    [Header("Debug")]
    public bool logOnceIfAnimatorNull = true;
    bool _logged;

    float _lastSpeedParam;
    bool _wasGrounded;

    public void Init(Animator a) { anim = a; }

    // ▶ PlayerController 인스펙터 값 동기화
    public void SyncSettings(float _dampUp, float _dampDown, float _stopSnap)
    {
        dampUp = _dampUp;
        dampDown = _dampDown;
        stopSnapThreshold = _stopSnap;
    }

    public void Tick(float dt, float planarSpeed, bool grounded, float vertical, bool jumpTriggered, bool isSprinting)
    {
        if (!anim)
        {
            if (logOnceIfAnimatorNull && !_logged)
            {
                Debug.Log("[PlayerAnimModule] Animator is null. Assign animatorSource on PlayerController.");
                _logged = true;
            }
            return;
        }

        // 1) Speed 파라미터: 스냅 + 감쇠
        float target = Mathf.Max(0f, planarSpeed) * speedToParam;

        if ((!snapOnlyWhenGrounded || grounded) && target <= stopSnapThreshold)
            target = 0f;

        float damp = (target >= _lastSpeedParam) ? dampUp : dampDown;
        if (!string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, target, damp, dt);
        _lastSpeedParam = target;

        // 2) 낙하 상태
        bool isFalling = !grounded && vertical < fallingThreshold;
        if (!string.IsNullOrEmpty(paramIsFalling))
            anim.SetBool(paramIsFalling, isFalling);

        // 3) 점프 트리거
        if (jumpTriggered && !string.IsNullOrEmpty(paramJump))
            anim.SetTrigger(paramJump);

        // 4) 착지 트리거
        if (!_wasGrounded && grounded && !string.IsNullOrEmpty(paramLand))
            anim.SetTrigger(paramLand);

        // 5) 스프린트 플래그
        if (!string.IsNullOrEmpty(paramIsSprinting))
            anim.SetBool(paramIsSprinting, isSprinting);

        _wasGrounded = grounded;
    }
}
