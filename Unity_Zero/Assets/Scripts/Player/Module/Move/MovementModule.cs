using System;
using UnityEngine;

[Serializable]
public class MovementModule
{
    // --- Runtime refs ---
    private CharacterController cc;
    private Transform tf;
    private StaminaModule stamina;

    // --- Tunables (컨트롤러에서 주입) ---
    // Move
    private float walkSpeed = 4f;
    private float sprintMultiplier = 1.7f;
    private KeyCode sprintKey = KeyCode.LeftShift;
    private bool sprintOnlyOnGround = true;

    // Jump Physics
    private float jumpHeight = 1.2f;
    private float gravity = -20f;
    private float groundedStick = -2f;

    // Jump Timing
    private float jumpCooldown = 0.2f;
    private float coyoteTime = 0.10f;
    private float jumpBufferTime = 0.10f;

    // Sprint Exhaust Stop
    private bool stopOnSprintExhaust = true;
    private float exhaustStopDuration = 0.2f;

    // --- State ---
    private Vector2 moveInput;
    private Vector3 planarVel;
    private float verticalVel;
    private bool grounded;
    private bool sprinting;
    private bool jumpTriggered;

    // Timers
    private float jumpCDTimer = 0f;
    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;
    private float exhaustStopTimer = 0f;
    private bool pendingGroundStop = false;

    // Init
    public void Initialize(CharacterController controller, Transform root, StaminaModule staminaModule)
    {
        cc = controller;
        tf = root;
        stamina = staminaModule;
        verticalVel = 0f;
        jumpCDTimer = 0f;
    }

    // Inspector → Module
    public void SyncSettings(
        // Jump Timing
        float s_jumpCooldown, float s_coyoteTime, float s_jumpBufferTime,
        // Move
        float s_walkSpeed, float s_sprintMultiplier, KeyCode s_sprintKey, bool s_sprintOnlyOnGround,
        // Jump Physics
        float s_jumpHeight, float s_gravity, float s_groundedStick,
        // Exhaust Stop
        bool s_stopOnExhaust, float s_exhaustStopDuration)
    {
        jumpCooldown = Mathf.Max(0f, s_jumpCooldown);
        coyoteTime = Mathf.Max(0f, s_coyoteTime);
        jumpBufferTime = Mathf.Max(0f, s_jumpBufferTime);

        walkSpeed = Mathf.Max(0f, s_walkSpeed);
        sprintMultiplier = Mathf.Max(1f, s_sprintMultiplier);
        sprintKey = s_sprintKey;
        sprintOnlyOnGround = s_sprintOnlyOnGround;

        jumpHeight = Mathf.Max(0f, s_jumpHeight);
        gravity = s_gravity;
        groundedStick = s_groundedStick;

        stopOnSprintExhaust = s_stopOnExhaust;
        exhaustStopDuration = Mathf.Max(0f, s_exhaustStopDuration);
    }

    // Tick
    public void Tick()
    {
        if (cc == null || tf == null) return;

        if (exhaustStopTimer > 0f) exhaustStopTimer -= Time.deltaTime;
        if (jumpCDTimer > 0f) jumpCDTimer -= Time.deltaTime;

        ReadInput();
        UpdateSprintState();

        // 공중에서 소진되었으면 착지 시 짧게 정지
        if (pendingGroundStop && cc.isGrounded)
        {
            exhaustStopTimer = Mathf.Max(exhaustStopTimer, exhaustStopDuration);
            pendingGroundStop = false;
        }

        MoveHorizontal();
        JumpAndGravity();
        ApplyMovement();
    }

    // -------------------- Input --------------------
    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // 점프 버퍼는 "지상" 또는 "하강 중"일 때만 기록 (상승 중 입력은 무시)
        if (Input.GetButtonDown("Jump"))
        {
            bool fallingOrGrounded = grounded || verticalVel <= 0f;
            if (fallingOrGrounded)
                lastJumpPressedTime = Time.time;
        }
    }

    // -------------------- Sprint --------------------
    void UpdateSprintState()
    {
        bool hasMoveInput = moveInput.sqrMagnitude > 0.001f;
        bool wantSprint = Input.GetKey(sprintKey);
        bool groundOk = !sprintOnlyOnGround || cc.isGrounded;
        bool staminaOk = (stamina == null) ? true : stamina.CanSprint();

        sprinting = wantSprint && hasMoveInput && groundOk && staminaOk;

        // 스프린트 중 스태미너 소진 → 강제 정지 트리거
        if (sprinting && stamina != null && !stamina.CanSprint())
        {
            sprinting = false;
            if (stopOnSprintExhaust)
            {
                if (!cc.isGrounded) pendingGroundStop = true; // 공중이면 착지 후 정지
                else exhaustStopTimer = Mathf.Max(exhaustStopTimer, exhaustStopDuration);
                moveInput = Vector2.zero; // 급감속
            }
        }
    }

    // -------------------- Horizontal --------------------
    void MoveHorizontal()
    {
        // Exhaust 정지 중에는 이동 입력 무시
        Vector2 useInput = (exhaustStopTimer > 0f) ? Vector2.zero : moveInput;

        Vector3 fwd = tf.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = tf.right; right.y = 0f; right.Normalize();

        Vector3 dir = (fwd * useInput.y + right * useInput.x);
        float speed = walkSpeed * (sprinting ? sprintMultiplier : 1f);
        planarVel = dir * speed;
    }

    // -------------------- Jump / Gravity --------------------
    void JumpAndGravity()
    {
        grounded = cc.isGrounded;
        jumpTriggered = false;

        if (grounded) lastGroundedTime = Time.time;

        if (grounded)
        {
            if (verticalVel < groundedStick) verticalVel = groundedStick;

            bool buffered = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
            bool cdReady = (jumpCDTimer <= 0f);

            if (exhaustStopTimer <= 0f && buffered && cdReady)
                TryJump();
        }
        else
        {
            bool withinCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
            bool buffered = (Time.time - lastJumpPressedTime) <= jumpBufferTime;
            bool cdReady = (jumpCDTimer <= 0f);

            if (exhaustStopTimer <= 0f && withinCoyote && buffered && cdReady)
                TryJump();

            verticalVel += gravity * Time.deltaTime;
        }
    }

    void TryJump()
    {
        bool staminaOk = (stamina == null || stamina.TrySpend(stamina.jumpCost));
        if (!staminaOk) return;

        DoJump();
    }

    void DoJump()
    {
        float g = Mathf.Abs(gravity);
        verticalVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
        jumpTriggered = true;
        jumpCDTimer = jumpCooldown;

        // ★ 버퍼 즉시 소모: 착지 후 ‘자동 점프’ 방지
        lastJumpPressedTime = -999f;
    }

    // -------------------- Apply --------------------
    void ApplyMovement()
    {
        Vector3 vel = new Vector3(planarVel.x, verticalVel, planarVel.z);
        cc.Move(vel * Time.deltaTime);
    }

    // -------------------- Queries --------------------
    public bool HasMoveInput() => moveInput.sqrMagnitude > 0.001f;
    public bool IsSprinting() => sprinting;
    public bool IsGrounded() => grounded;
    public float GetPlanarSpeed() => planarVel.magnitude;
    public float GetVerticalVelocity() => verticalVel;

    public bool ConsumeJumpTriggered()
    {
        bool v = jumpTriggered;
        jumpTriggered = false;
        return v;
    }

    public void ForceStopSprint()
    {
        sprinting = false;
        if (!stopOnSprintExhaust) return;

        if (!cc.isGrounded) pendingGroundStop = true;
        else exhaustStopTimer = Mathf.Max(exhaustStopTimer, exhaustStopDuration);
        moveInput = Vector2.zero;
    }
}
