using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;          // 걷기 속도
    public float runSpeed = 6.5f;          // 뛰기 속도
    public float rotationSpeed = 8f;        // 회전 스무싱

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;         // 점프 높이
    public float gravity = -20f;         // 중력 가속도
    public float fallYVelThreshold = -0.1f; // 이 값보다 떨어지면 낙하로 판단

    private CharacterController controller;
    private Vector3 velocity;               // 수직 속도만 담당
    private bool lastGrounded;              // 직전 프레임의 접지 상태

    [Header("Animation")]
    [SerializeField] private Animator anim; // Animator
    [SerializeField] private float speedDamp = 0.1f; // Speed 감쇠(일반)

    [Header("Coyote & Cooldown")]
    [SerializeField] private float coyoteTime = 0.12f; // 땅을 벗어난 직후 잠깐 점프 허용
    [SerializeField] private float jumpCooldown = 0.05f; // 점프 연타 방지
    private float coyoteTimer = 0f;
    private float jumpCDTimer = 0f;

    [Header("Jump Assist (Buffer & Post-Land Grace)")]
    [SerializeField] float jumpBufferTime = 0.15f;   // 점프 키를 미리 눌러도 유효한 버퍼 시간
    [SerializeField] float postLandGrace = 0.5f;    // 착지 후 이 시간 동안엔 점프 바로 허용
    float jumpBufferTimer = 0f;
    float postLandTimer = 0f;


    [Header("Stamina")]
    [SerializeField] private PlayerStamina stamina; // 스태미나 참조



    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!stamina) stamina = GetComponent<PlayerStamina>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- 입력 ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space);

        // 점프 입력 버퍼 (키를 ‘조금 일찍’ 눌러도 저장)
        if (jumpKeyDown) jumpBufferTimer = jumpBufferTime;


        // 카메라 기준 이동 방향
        Vector3 camF = cameraTransform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = cameraTransform.right; camR.y = 0; camR.Normalize();
        Vector3 moveDir = (camF * v + camR * h);
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude > 0.0001f;

        // ---------- 스프린트 판정(순서 중요!) ----------
        bool wantSprint = sprintKey && hasMoveInput;                                 // 스프린트 의도
        bool isSprinting = wantSprint && (stamina == null || stamina.CanStartSprint());// 시작 가능?
        if (isSprinting && stamina != null)
        {
            // 프레임당 소모. 0이 되면 즉시 중단.
            if (!stamina.DrainSprintTick()) isSprinting = false;
        }
        //스프린트가 아니고, 이동 입력이 있을 때는 '걷기 소모'
        if (!isSprinting && hasMoveInput && stamina != null)
        {
            stamina.DrainWalkTick(); // 0이 되어도 걷기는 허용, 단 스프린트는 못함
        }

        // 🔴 위에서 최종 isSprinting이 확정된 뒤에 속도 결정해야 한다!
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        // ---------- 수평 이동 ----------
        Vector3 horizontalMove = moveDir.normalized * currentSpeed * Time.deltaTime;
        controller.Move(horizontalMove);

        // ---------- 회전 ----------
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // ---------- 접지 체크 & 코요테 ----------
        bool groundedNow = controller.isGrounded;
        if (groundedNow)
        {
            coyoteTimer = coyoteTime;                  // 착지 시 버퍼 리필
            if (velocity.y < 0f) velocity.y = -2f;     // 바닥에 붙이기(떨림 방지)
        }
        else
        {
            coyoteTimer -= Time.deltaTime;             // 공중이면 버퍼 감소
        }

        if (!lastGrounded && groundedNow)
        {
            postLandTimer = postLandGrace; // 착지 직후 0.5초 윈도우 오픈
        }

        // ---------- 점프 ----------
        // 타이머 감소
        jumpCDTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        postLandTimer -= Time.deltaTime;

        // 점프 가능 조건: (접지) 또는 (코요테) 또는 (착지 그레이스)  AND 쿨타임 완료 AND 버퍼에 입력 있음
        bool canJumpNow = (groundedNow || coyoteTimer > 0f || postLandTimer > 0f) && (jumpCDTimer <= 0f);
        bool hasBufferedJump = (jumpBufferTimer > 0f);

        if (hasBufferedJump && canJumpNow)
        {
            // 스태미나 체크/소모
            bool ok = (stamina == null) ? true : stamina.TrySpend(stamina.jumpCost);
            if (ok)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim?.SetTrigger("Jump");

                // 타이머/상태 초기화
                jumpCDTimer = jumpCooldown;
                jumpBufferTimer = 0f;      // 버퍼 소진
                coyoteTimer = 0f;      // 공중으로 전환
                postLandTimer = 0f;      // 착지 그레이스 종료
                groundedNow = false;
            }
        }


        // ---------- 중력 & 수직 이동 ----------
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ---------- 낙하/착지 애니 ----------
        bool isFalling = velocity.y < fallYVelThreshold && !controller.isGrounded;
        anim?.SetBool("IsFalling", isFalling);

        // 직전 프레임 공중 → 이번 프레임 접지 = 착지
        if (!lastGrounded && controller.isGrounded)
        {
            anim?.SetTrigger("Land");
            if (velocity.y < 0f) velocity.y = -2f;
        }
        lastGrounded = controller.isGrounded;

        // ---------- 애니메이션 파라미터 ----------
        // --- Animator 이동 파라미터 (스냅 방식: Idle=0, Walk=0.5, Run=1) ---
        float inputMag = new Vector2(h, v).magnitude;  // 0~1
        float speed01;

        if (inputMag < 0.05f)          // 멈춤
        {
            speed01 = 0f;
            isSprinting = false;       // 정지 시 스프린트 해제
        }
        else if (isSprinting)          // 달림
        {
            speed01 = 1f;
        }
        else                            // 걷기
        {
            speed01 = 0.5f;            // Walk 임계값으로 '딱' 고정
        }

        if (anim)
        {
            // 멈출 때는 더 빠르게 0으로 스냅
            float damp = (speed01 == 0f) ? 0.03f : speedDamp;
            anim.SetFloat("Speed", speed01, damp, Time.deltaTime);
            anim.SetBool("IsSprinting", isSprinting);
        }


        // 감쇠: 멈출 때는 빠르게 스냅
        if (anim)
        {
            float damp = (speed01 == 0f) ? 0.03f : speedDamp;
            anim.SetFloat("Speed", speed01, damp, Time.deltaTime);
            anim.SetBool("IsSprinting", isSprinting); // 참고용(전이 조건엔 사용 X)
        }
    }
}
