using System;
using UnityEngine;

[Serializable]
public class MovementModule
{
    // 런타임 참조
    PlayerController _playerController;
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

    // 상태 값
    Vector2 _moveInput;
    Vector3 _planarVel;
    float _verticalVel;
    bool _grounded;
    bool _sprinting;
    bool _jumpTriggered;

    // 착지 후 잠깐 이동 락
    float _landingLockDuration = 0.4f; // 원하는 값으로 조정 (0.2~0.4 정도)
    float _landingLockTimer;

    // 타이머
    float _jumpCDTimer;
    float _lastGroundedTime = -999f;
    float _lastJumpPressedTime = -999f;
    float _exhaustStopTimer;
    bool _pendingGroundStop;

    private float _speedMultiplier = 1f;   // 기본 1.0

    public void SetSpeedMultiplier(float value)
    {
        _speedMultiplier = Mathf.Max(0f, value);   // 0 미만으로는 안 떨어지게
    }

    // 초기화: 필수 참조 연결
    public void Initialize(PlayerController player, CharacterController controller, Transform root, EfficiencyModule efficiencyModule)
    {
        _playerController = player;
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
        float exhaustStopDuration,
        float landingLockDuration
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

        _landingLockDuration = Mathf.Max(0f, landingLockDuration);
    }

    // 매 프레임 이동 처리
    public void Tick()
    {
        if (_playerController != null && _playerController.IsRewinding)
            return;

        if (_cc == null || _tf == null)
            return;

        if (_exhaustStopTimer > 0f)
            _exhaustStopTimer -= Time.deltaTime;

        if (_jumpCDTimer > 0f)
            _jumpCDTimer -= Time.deltaTime;

        if (_landingLockTimer > 0f)
            _landingLockTimer -= Time.deltaTime;

        ReadInput();

        UpdateSprintState();

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
            // 착지 락 중엔 점프 버퍼 기록 금지
            if (fallingOrGrounded && _landingLockTimer <= 0f)
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
        Vector2 useInput = (_exhaustStopTimer > 0f || _landingLockTimer > 0f)
            ? Vector2.zero
            : _moveInput;

        Vector3 fwd = _tf.forward;
        fwd.y = 0f;
        fwd.Normalize();

        Vector3 right = _tf.right;
        right.y = 0f;
        right.Normalize();

        Vector3 dir = (fwd * useInput.y + right * useInput.x);

        float baseSpeed = _walkSpeed * (_sprinting ? _sprintMultiplier : 1f);
        float speed = baseSpeed * _speedMultiplier;

        _planarVel = dir * speed;
    }


    // 점프와 중력 처리
    void JumpAndGravity()
    {
        bool wasGrounded = _grounded;
        _grounded = _cc.isGrounded;
        _jumpTriggered = false;

        if (_grounded)
        {
            // 공중 → 지상으로 바뀐 첫 프레임에 락 시작
            if (!wasGrounded && _landingLockDuration > 0f)
                _landingLockTimer = _landingLockDuration;

            if (_verticalVel < _groundedStick)
                _verticalVel = _groundedStick;

            _lastGroundedTime = Time.time;

            bool buffered = (Time.time - _lastJumpPressedTime) <= _jumpBufferTime;
            bool cdReady = (_jumpCDTimer <= 0f);

            if (_exhaustStopTimer <= 0f && _landingLockTimer <= 0f && buffered && cdReady)
                TryJump();
        }
        else
        {
            bool withinCoyote = (Time.time - _lastGroundedTime) <= _coyoteTime;
            bool buffered = (Time.time - _lastJumpPressedTime) <= _jumpBufferTime;
            bool cdReady = (_jumpCDTimer <= 0f);

            if (_exhaustStopTimer <= 0f && withinCoyote && buffered && cdReady)
                TryJump();

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
        if (_cc == null || !_cc.enabled)
            return;                      // 컨트롤러 꺼져 있으면 Move 안 함

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

    public void SetWalkSpeed(float speed)
    {
        _walkSpeed = speed;
    }

}
