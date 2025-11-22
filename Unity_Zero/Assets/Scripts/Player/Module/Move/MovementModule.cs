using System;
using UnityEngine;

[Serializable]
public class MovementModule
{
    // 런타임 참조
    CharacterController _cc;
    Transform _tf;
    EfficiencyModule _efficiency;

    // 이동 튜닝 값
    float _walkSpeed = 4f;
    float _sprintMultiplier = 1.7f;
    KeyCode _sprintKey = KeyCode.LeftShift;
    bool _sprintOnlyOnGround = true;

    // 점프 및 중력
    float _jumpHeight = 1.2f;
    float _gravity = -20f;
    float _groundedStick = -2f;

    // 점프 타이밍
    float _jumpCooldown = 0.2f;
    float _coyoteTime = 0.1f;
    float _jumpBufferTime = 0.1f;

    // 스프린트 소진 후 정지 관련
    bool _stopOnSprintExhaust = true;
    float _exhaustStopDuration = 0.2f;

    // 상태 값
    Vector2 _moveInput;
    Vector3 _planarVel;
    float _verticalVel;
    bool _grounded;
    bool _sprinting;
    bool _jumpTriggered;

    // 타이머
    float _jumpCDTimer;
    float _lastGroundedTime = -999f;
    float _lastJumpPressedTime = -999f;
    float _exhaustStopTimer;
    bool _pendingGroundStop;

    // 초기화: 필수 참조 연결
    public void Initialize(CharacterController controller, Transform root, EfficiencyModule efficiencyModule)
    {
        _cc = controller;
        _tf = root;
        _efficiency = efficiencyModule;
        _verticalVel = 0f;
        _jumpCDTimer = 0f;
    }

    // PlayerController 인스펙터 값 주입
    public void SyncSettings(
        float jumpCooldown,
        float coyoteTime,
        float jumpBufferTime,
        float walkSpeed,
        float sprintMultiplier,
        KeyCode sprintKey,
        bool sprintOnlyOnGround,
        float jumpHeight,
        float gravity,
        float groundedStick,
        bool stopOnExhaust,
        float exhaustStopDuration
    )
    {
        _jumpCooldown = Mathf.Max(0f, jumpCooldown);
        _coyoteTime = Mathf.Max(0f, coyoteTime);
        _jumpBufferTime = Mathf.Max(0f, jumpBufferTime);

        _walkSpeed = Mathf.Max(0f, walkSpeed);
        _sprintMultiplier = Mathf.Max(1f, sprintMultiplier);
        _sprintKey = sprintKey;
        _sprintOnlyOnGround = sprintOnlyOnGround;

        _jumpHeight = Mathf.Max(0f, jumpHeight);
        _gravity = gravity;
        _groundedStick = groundedStick;

        _stopOnSprintExhaust = stopOnExhaust;
        _exhaustStopDuration = Mathf.Max(0f, exhaustStopDuration);
    }

    // 매 프레임 이동 처리
    public void Tick()
    {
        if (_cc == null || _tf == null)
            return;

        if (_exhaustStopTimer > 0f)
            _exhaustStopTimer -= Time.deltaTime;

        if (_jumpCDTimer > 0f)
            _jumpCDTimer -= Time.deltaTime;

        ReadInput();
        UpdateSprintState();

        // 공중에서 소진된 상태였다면 착지 시 잠시 정지
        if (_pendingGroundStop && _cc.isGrounded)
        {
            _exhaustStopTimer = Mathf.Max(_exhaustStopTimer, _exhaustStopDuration);
            _pendingGroundStop = false;
        }

        MoveHorizontal();
        JumpAndGravity();
        ApplyMovement();
    }

    // 이동 및 점프 입력 처리
    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(h, v).normalized;

        // 점프 입력 버퍼링 (상승 중 입력은 무시)
        if (Input.GetButtonDown("Jump"))
        {
            bool fallingOrGrounded = _grounded || _verticalVel <= 0f;
            if (fallingOrGrounded)
                _lastJumpPressedTime = Time.time;
        }
    }

    // 스프린트 상태 갱신
    void UpdateSprintState()
    {
        bool hasMoveInput = _moveInput.sqrMagnitude > 0.001f;
        bool wantSprint = Input.GetKey(_sprintKey);
        bool groundOk = !_sprintOnlyOnGround || _cc.isGrounded;

        _sprinting = wantSprint && hasMoveInput && groundOk;
    }

    // 수평 이동 벡터 계산
    void MoveHorizontal()
    {
        // 소진 정지 중에는 입력 무시
        Vector2 useInput = (_exhaustStopTimer > 0f) ? Vector2.zero : _moveInput;

        Vector3 fwd = _tf.forward;
        fwd.y = 0f;
        fwd.Normalize();

        Vector3 right = _tf.right;
        right.y = 0f;
        right.Normalize();

        Vector3 dir = (fwd * useInput.y + right * useInput.x);
        float speed = _walkSpeed * (_sprinting ? _sprintMultiplier : 1f);

        _planarVel = dir * speed;
    }

    // 점프와 중력 처리
    void JumpAndGravity()
    {
        _grounded = _cc.isGrounded;
        _jumpTriggered = false;

        if (_grounded)
            _lastGroundedTime = Time.time;

        if (_grounded)
        {
            // 지면에 단단히 붙게 하는 보정
            if (_verticalVel < _groundedStick)
                _verticalVel = _groundedStick;

            bool buffered = (Time.time - _lastJumpPressedTime) <= _jumpBufferTime;
            bool cdReady = (_jumpCDTimer <= 0f);

            if (_exhaustStopTimer <= 0f && buffered && cdReady)
                TryJump();
        }
        else
        {
            bool withinCoyote = (Time.time - _lastGroundedTime) <= _coyoteTime;
            bool buffered = (Time.time - _lastJumpPressedTime) <= _jumpBufferTime;
            bool cdReady = (_jumpCDTimer <= 0f);

            if (_exhaustStopTimer <= 0f && withinCoyote && buffered && cdReady)
                TryJump();

            // 공중 중력 적용
            _verticalVel += _gravity * Time.deltaTime;
        }
    }

    // 점프 시도: 효율 소모 요청 후 실제 점프
    void TryJump()
    {
        if (_efficiency != null)
        {
            // 효율이 남아 있다면 그만큼만 소모 (부족해도 행동은 허용)
            _efficiency.TrySpend(_efficiency.jumpCost);
        }

        DoJump();
    }

    // 실제 점프 속도 적용
    void DoJump()
    {
        float g = Mathf.Abs(_gravity);
        _verticalVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, _jumpHeight));
        _jumpTriggered = true;
        _jumpCDTimer = _jumpCooldown;

        // 버퍼 초기화로 자동 점프 방지
        _lastJumpPressedTime = -999f;
    }

    // CharacterController에 최종 이동 적용
    void ApplyMovement()
    {
        Vector3 vel = new Vector3(_planarVel.x, _verticalVel, _planarVel.z);
        _cc.Move(vel * Time.deltaTime);
    }

    // 외부에서 조회용
    public bool HasMoveInput() => _moveInput.sqrMagnitude > 0.001f;
    public bool IsSprinting() => _sprinting;
    public bool IsGrounded() => _grounded;
    public float GetPlanarSpeed() => _planarVel.magnitude;
    public float GetVerticalVelocity() => _verticalVel;

    // 점프 트리거를 한 번만 소비
    public bool ConsumeJumpTriggered()
    {
        bool v = _jumpTriggered;
        _jumpTriggered = false;
        return v;
    }

    // 효율 시스템에서 강제 스프린트 정지 요청 시 사용 가능
    public void ForceStopSprint()
    {
        _sprinting = false;

        if (!_stopOnSprintExhaust)
            return;

        if (!_cc.isGrounded)
            _pendingGroundStop = true;
        else
            _exhaustStopTimer = Mathf.Max(_exhaustStopTimer, _exhaustStopDuration);

        _moveInput = Vector2.zero;
    }
}
