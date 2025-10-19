using UnityEngine;

[RequireComponent(typeof(CharacterController))] // �ݵ�� CharacterController�� �ʿ��ϴٴ� ����
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;   // �ȱ� �ӵ�
    [SerializeField] private float jumpHeight = 1.2f; // ���� ����(���ϸ� 0����)
    [SerializeField] private float gravity = -9.81f;  // �߷� ���ӵ�(�Ʒ� �����̹Ƿ� ����)

    private CharacterController controller; // �浹/���/������ ó�����ִ� ����Ƽ ������Ʈ
    private Vector3 velocity;               // ���� �������� �ӵ� ����(Ư�� y�� �߷�/������)

    private void Awake()
    {
        controller = GetComponent<CharacterController>(); // ���� ������Ʈ���� ������Ʈ ��������
    }

    private void Update()
    {
        // 1) �Է� �ޱ� (Unity �⺻ �Է�: Horizontal=A/D, Vertical=W/S)
        float h = Input.GetAxisRaw("Horizontal"); // -1~1: ��(-) ����(+)
        float v = Input.GetAxisRaw("Vertical");   // -1~1: ��(-) ��(+)

        // 2) ī�޶� ���� ���� ���ϱ�(ī�޶� ������ �÷��̾� ����)
        Vector3 fwd = Camera.main ? Camera.main.transform.forward : transform.forward; // ����
        Vector3 right = Camera.main ? Camera.main.transform.right : transform.right;   // ����
        fwd.y = 0f; right.y = 0f;      // ����� ����(���� ������ ����)
        fwd.Normalize(); right.Normalize(); // ���� ���ͷ� ����ȭ

        // 3) �̵� ���� ���ϱ�(����/�¿� �Է� ��ġ�� �� ����ȭ �� �ӵ� ��)
        Vector3 move = (fwd * v + right * h).normalized * moveSpeed;

        // 4) ���� �̵� ����(deltaTime�� ���� ������ ����)
        controller.Move(move * Time.deltaTime);

        // 5) ���� üũ & ���� ó��
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // �ٴڿ� �� �ٿ��ִ� ����(0�̸� ��Ȥ ���� ���� Ʀ)

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // v^2 = 2gh ���� ����

        // 6) �߷� ����(�����Ӹ��� y�ӵ� ����)
        velocity.y += gravity * Time.deltaTime;

        // 7) ���� �̵� ����
        controller.Move(velocity * Time.deltaTime);
    }
}
