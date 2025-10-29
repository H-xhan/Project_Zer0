using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform;   // 메인 카메라 Transform
    public float moveSpeed = 5f;        // 이동 속도
    public float rotationSpeed = 8f;    // 회전 속도
    public float jumpHeight = 1.2f;     // 점프 높이
    public float gravity = -20f;        // 중력 값

    private CharacterController controller;
    private Vector3 velocity;           // 수직 속도 (중력용)
    private bool isGrounded;            // 땅 체크용

    void Start()
    {
        controller = GetComponent<CharacterController>();      // 캐릭터 컨트롤러 가져오기
        if (!cameraTransform)
            cameraTransform = Camera.main.transform;           // 카메라 자동 참조
        Cursor.lockState = CursorLockMode.Locked;              // 마우스 커서 고정
    }

    void Update()
    {
        // --- 이동 입력 ---
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S
        Vector3 move = new Vector3(h, 0, v);

        // 카메라 방향 기준으로 변환
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        // 이동 적용
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // --- 회전 ---
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // --- 점프 / 중력 ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // 땅에 붙여줌

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // 점프

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
