using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 신 Input System 지원
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;      // 이동 속도
    [SerializeField] private float jumpHeight = 1.2f;   // 점프 높이 (단위: 미터)
    [SerializeField] private float gravity = -9.81f;    // 중력 가속도
    [SerializeField] private float groundCheckDistance = 0.2f; // 지면 판정 거리
    [SerializeField] private LayerMask groundMask;      // 지면 레이어 지정용

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1️⃣ 이동 입력 (신 Input System / 구 Input 둘 다 지원)
        float h = 0f, v = 0f;
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed) h += 1f;
            if (kb.sKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed) v += 1f;
            jumpPressed = kb.spaceKey.wasPressedThisFrame;
        }
#else
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
#endif

        // 2️⃣ 카메라 기준 이동 벡터 계산
        Vector3 fwd = Camera.main ? Camera.main.transform.forward : transform.forward;
        Vector3 right = Camera.main ? Camera.main.transform.right : transform.right;
        fwd.y = 0f; right.y = 0f; // 수평 이동만
        fwd.Normalize(); right.Normalize();
        Vector3 move = (fwd * v + right * h).normalized * moveSpeed;
        controller.Move(move * Time.deltaTime);

        // 3️⃣ 지면 판정 (바닥과의 거리 체크)
        isGrounded = Physics.CheckSphere(transform.position + Vector3.down * 0.9f, groundCheckDistance, groundMask);

        // 지면에 닿으면 y속도 리셋
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // 4️⃣ 점프 입력
        if (jumpPressed && isGrounded)
        {
            // 점프 속도 계산 (역학 공식 v = sqrt(2gh))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5️⃣ 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

