using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Speed Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    public float rotationSpeed = 8f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    public float fallYVelThreshold = -0.1f; // 이 값보다 떨어지면 낙하로 판단

    CharacterController controller;
    Vector3 velocity;          // 수직속도만 담당
    bool isGrounded, wasGrounded;

    [Header("Animation")]
    [SerializeField] Animator anim;
    [SerializeField] float speedDamp = 0.1f;

    [SerializeField] float coyoteTime = 0.12f;   // 땅을 벗어난 직후 잠깐 점프 허용
    [SerializeField] float jumpCooldown = 0.05f; // 점프 연타 방지
    float coyoteTimer = 0f;
    float jumpCDTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;

        if (anim == null) anim = GetComponentInChildren<Animator>(true);
    }

    void Update()
    {
        // --- 입력 ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        // 카메라 기준 이동방향
        Vector3 camF = cameraTransform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = cameraTransform.right; camR.y = 0; camR.Normalize();
        Vector3 moveDir = (camF * v + camR * h);
        bool hasMoveInput = new Vector2(h, v).sqrMagnitude > 0.0001f;
        bool isSprinting = sprintKey && hasMoveInput;
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        // ① 수평 이동(첫 번째 Move)
        Vector3 horizontalMove = moveDir.normalized * currentSpeed * Time.deltaTime;
        controller.Move(horizontalMove);

        // ② 회전
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // --- Ground 상태 갱신 (Move 이후에도 다시 확인) ---
        bool groundedNow = controller.isGrounded;
        if (groundedNow)
        {
            coyoteTimer = coyoteTime;                 // 착지하면 버퍼 리필
            if (velocity.y < 0f) velocity.y = -2f;    // 바닥에 붙이기(떨림 방지)
        }
        else
        {
            coyoteTimer -= Time.deltaTime;            // 공중이면 버퍼 감소
        }

        // --- 점프 입력 처리 (버퍼 + 쿨타임) ---
        jumpCDTimer -= Time.deltaTime;
        if (jumpPressed && coyoteTimer > 0f && jumpCDTimer <= 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (anim) anim.SetTrigger("Jump");
            jumpCDTimer = jumpCooldown;               // 연타로 중복점프 방지
            groundedNow = false;                      // 바로 공중 상태로 전환
            coyoteTimer = 0f;
        }

        // ③ 중력 & 수직 이동(두 번째 Move)
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 낙하/착지 애니 파라미터
        bool isFalling = velocity.y < -0.1f && !controller.isGrounded;
        if (anim) anim.SetBool("IsFalling", isFalling);

        // 방금 프레임에 착지했는지 체크 (직전 Move 이후 값으로 판단)
        if (!groundedNow && controller.isGrounded)
        {
            if (anim) anim.SetTrigger("Land");
            if (velocity.y < 0f) velocity.y = -2f;
        }

        // --- Animator 이동 파라미터 ---
        float worldSpeed = (moveDir.normalized * currentSpeed).magnitude;
        float speed01 = Mathf.Clamp01(worldSpeed / runSpeed);
        if (anim)
        {
            anim.SetFloat("Speed", speed01, speedDamp, Time.deltaTime);
            anim.SetBool("IsSprinting", isSprinting);
        }
    }
}
