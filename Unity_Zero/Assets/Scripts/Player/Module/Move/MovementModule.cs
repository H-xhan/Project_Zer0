using UnityEngine;

[System.Serializable]
public class MovementModule
{
    [Header("Movement")]
    public float walkSpeed = 4.0f;
    public float sprintMultiplier = 1.7f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool sprintOnlyOnGround = true;

    [Header("Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -20.0f;
    public float groundedStick = -2.0f;

    [Header("Sprint Exhaust Stop")]
    public bool stopOnSprintExhaust = true;    // 소진 시 정지할지
    public float exhaustStopDuration = 0.20f;  // 정지 유지 시간(초)

    private float _exhaustStopTimer = 0f;      // 남은 정지 시간
    private bool _pendingGroundStop = false;  // 공중에서 소진된 경우, 착지 시 정지 예약

    // internal
    private CharacterController _cc;
    private Transform _transform;
    private StaminaModule _stamina;  // optional
    private Vector3 _input;
    private Vector3 _horizontal;
    private float _vertical;
    private bool _isSprinting;
    private bool _jumpTriggered; // expose once-per-frame

    public void Initialize(CharacterController cc, Transform t, StaminaModule stamina = null)
    {
        _cc = cc;
        _transform = t;
        _stamina = stamina;
    }

    public void Tick()
    {
        if (_cc == null || _transform == null) return;

        // 소진 정지 타이머 감소
        if (_exhaustStopTimer > 0f)
            _exhaustStopTimer -= Time.deltaTime;

        ReadInput();
        UpdateSprintState();

        // ★ 공중에서 소진되었다면, "이번 프레임에 땅을 밟은 경우" 정지 시작
        if (_pendingGroundStop && _cc.isGrounded)
        {
            _exhaustStopTimer = Mathf.Max(_exhaustStopTimer, exhaustStopDuration);
            _pendingGroundStop = false;
        }

        MoveHorizontal();
        JumpAndGravity();
        ApplyMovement();
    }

    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _input = new Vector3(h, 0f, v);
        _input = Vector3.ClampMagnitude(_input, 1f);
    }

    void UpdateSprintState()
    {
        bool hasMoveInput = _input.sqrMagnitude > 0.001f;
        bool wantSprint = Input.GetKey(sprintKey);
        bool groundOk = !sprintOnlyOnGround || _cc.isGrounded;
        bool staminaOk = (_stamina == null) ? true : _stamina.CanSprint(); // ★ 스태미너 부족이면 스프린트 불가

        _isSprinting = wantSprint && hasMoveInput && groundOk && staminaOk;
    }

    void MoveHorizontal()
    {
        Vector3 fwd = _transform.forward;
        Vector3 right = _transform.right;
        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 dir = (_exhaustStopTimer > 0f) ? Vector3.zero
                                               : (fwd * _input.z + right * _input.x);
        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = walkSpeed * (_isSprinting ? sprintMultiplier : 1f);
        _horizontal = dir * speed; // 수평 속도
    }

    void JumpAndGravity()
    {
        bool grounded = _cc.isGrounded;
        _jumpTriggered = false;

        if (grounded)
        {
            if (_vertical < groundedStick) _vertical = groundedStick;

            // 정지 타이머 동안 "점프 입력"만 막고, 중력은 그대로
            if (_exhaustStopTimer <= 0f && Input.GetButtonDown("Jump"))
            {
                bool staminaOk = true;
                if (_stamina != null && _stamina.jumpCost > 0f)
                    staminaOk = _stamina.TrySpend(_stamina.jumpCost);

                if (staminaOk)
                {
                    float g = Mathf.Abs(gravity);
                    _vertical = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
                    _jumpTriggered = true;
                }
            }
        }
        else
        {
            // ★ 공중에서는 항상 중력 적용 (멈추지 않게)
            _vertical += gravity * Time.deltaTime;
        }
    }

    void ApplyMovement()
    {
        Vector3 velocity = new Vector3(_horizontal.x, _vertical, _horizontal.z);
        _cc.Move(velocity * Time.deltaTime);
    }

    // getters
    public bool IsSprinting() { return _isSprinting; }
    public bool IsGrounded() { return _cc != null && _cc.isGrounded; }
    public float GetPlanarSpeed() { return _horizontal.magnitude; }
    public Vector3 GetMoveDirection() { return _horizontal.normalized; }
    public float GetVerticalVelocity() { return _vertical; }
    public bool ConsumeJumpTriggered() { bool v = _jumpTriggered; _jumpTriggered = false; return v; }
    public bool HasMoveInput() { return _input.sqrMagnitude > 0.001f; }

    public void ForceStopSprint()
    {
        _isSprinting = false;

        if (!stopOnSprintExhaust) return;

        // 공중이면 바로 멈추지 말고 "착지 때" 멈추도록 예약
        if (_cc != null && !_cc.isGrounded)
        {
            _pendingGroundStop = true; // 다음에 땅 밟는 프레임에 정지 시작
        }
        else
        {
            _exhaustStopTimer = Mathf.Max(_exhaustStopTimer, exhaustStopDuration);
        }
    }
}
