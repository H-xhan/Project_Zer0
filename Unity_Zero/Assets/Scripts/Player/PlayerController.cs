using UnityEngine;

[RequireComponent(typeof(CharacterController))] // 반드시 CharacterController가 필요하다는 선언
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;   // 걷기 속도
    [SerializeField] private float jumpHeight = 1.2f; // 점프 높이(원하면 0으로)
    [SerializeField] private float gravity = -9.81f;  // 중력 가속도(아래 방향이므로 음수)

    private CharacterController controller; // 충돌/경사/스텝을 처리해주는 유니티 컴포넌트
    private Vector3 velocity;               // 현재 프레임의 속도 누적(특히 y축 중력/점프용)

    private void Awake()
    {
        controller = GetComponent<CharacterController>(); // 같은 오브젝트에서 컴포넌트 가져오기
    }

    private void Update()
    {
        // 1) 입력 받기 (Unity 기본 입력: Horizontal=A/D, Vertical=W/S)
        float h = Input.GetAxisRaw("Horizontal"); // -1~1: 왼(-) 오른(+)
        float v = Input.GetAxisRaw("Vertical");   // -1~1: 뒤(-) 앞(+)

        // 2) 카메라 기준 방향 구하기(카메라가 없으면 플레이어 기준)
        Vector3 fwd = Camera.main ? Camera.main.transform.forward : transform.forward; // 전방
        Vector3 right = Camera.main ? Camera.main.transform.right : transform.right;   // 우측
        fwd.y = 0f; right.y = 0f;      // 수평면 투영(상하 기울어짐 제거)
        fwd.Normalize(); right.Normalize(); // 단위 벡터로 정규화

        // 3) 이동 벡터 구하기(전후/좌우 입력 합치기 → 정규화 → 속도 곱)
        Vector3 move = (fwd * v + right * h).normalized * moveSpeed;

        // 4) 수평 이동 적용(deltaTime을 곱해 프레임 독립)
        controller.Move(move * Time.deltaTime);

        // 5) 지면 체크 & 점프 처리
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // 바닥에 딱 붙여주는 보정(0이면 간혹 공중 판정 튐)

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // v^2 = 2gh 공식 변형

        // 6) 중력 누적(프레임마다 y속도 감소)
        velocity.y += gravity * Time.deltaTime;

        // 7) 수직 이동 적용
        controller.Move(velocity * Time.deltaTime);
    }
}
