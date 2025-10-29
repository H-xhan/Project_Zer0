using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 2, -4); // 카메라 위치 오프셋
    public float smoothSpeed = 10f;   // 부드러운 이동 속도
    public float mouseSensitivity = 150f; // 마우스 감도
    public float minPitch = -30f;     // 상하 최소 각도
    public float maxPitch = 60f;      // 상하 최대 각도

    private float yaw = 0f;           // 좌우 회전 각도
    private float pitch = 10f;        // 상하 회전 각도 (기본 시선 약간 아래)

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 잠금
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- 마우스 입력 ---
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity * Time.deltaTime;     // 좌우 회전
        pitch -= mouseY * mouseSensitivity * Time.deltaTime;   // 상하 회전 (위로 올릴수록 마이너스)
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);        // 시점 제한

        // --- 회전 계산 ---
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // --- 위치 계산 ---
        Vector3 desiredPos = target.position + rotation * offset;

        // --- 카메라 부드럽게 이동 + 타깃 보기 ---
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
