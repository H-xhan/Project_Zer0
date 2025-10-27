using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // 플레이어 루트 (CharacterController 붙은 오브젝트)

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 1.8f, -3.5f); // (X: 어깨, Y: 높이, Z: 뒤 거리)

    [Header("Mouse Settings")]
    [SerializeField] private float sensitivityX = 3.5f; // 좌우 회전 속도
    [SerializeField] private float sensitivityY = 2f;   // 상하 회전 속도
    [SerializeField] private float minPitch = -45f;     // 위 보기 제한
    [SerializeField] private float maxPitch = 70f;      // 아래 보기 제한
    [SerializeField] private bool invertY = false;

    [Header("Smoothing")]
    [SerializeField] private float followSmooth = 0.1f;
    [SerializeField] private float rotationSmooth = 12f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.25f;

    private float yaw;    // 마우스 X 누적(좌우)
    private float pitch;  // 마우스 Y 누적(상하)
    private Vector3 posVel;

    private void Start()
    {
        if (!target)
        {
            Debug.LogWarning("[OverwatchCamera] Target not assigned.");
            enabled = false; return;
        }

        // 초기 각도 세팅
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            else
            { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
    }

    private void LateUpdate()
    {
        if (!target || !Application.isFocused) return;

        // --- 1. 마우스 델타 입력 읽기 ---
        Vector2 delta = ReadMouseDelta();
        yaw += delta.x * sensitivityX;
        pitch += (invertY ? delta.y : -delta.y) * sensitivityY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // --- 2. 회전 계산 ---
        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);

        // --- 3. 플레이어는 카메라 Yaw 방향으로 회전 ---
        Quaternion targetYaw = Quaternion.Euler(0f, yaw, 0f);
        target.rotation = Quaternion.Slerp(target.rotation, targetYaw, Time.deltaTime * rotationSmooth);

        // --- 4. 카메라 목표 위치 계산 ---
        Vector3 desired = target.position
                        + target.rotation * new Vector3(offset.x, offset.y, offset.z);

        // --- 5. 충돌 보정 (벽에 박히지 않게) ---
        Vector3 pivot = target.position + Vector3.up * offset.y;
        Vector3 dir = desired - pivot;
        float dist = dir.magnitude;
        if (Physics.SphereCast(pivot, collisionRadius, dir.normalized, out RaycastHit hit, dist, collisionMask))
        {
            desired = hit.point - dir.normalized * 0.05f;
        }

        // --- 6. 부드럽게 이동 & 회전 ---
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref posVel, followSmooth);
        transform.rotation = camRot;
    }

    private Vector2 ReadMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        var m = Mouse.current;
        return (m != null) ? m.delta.ReadValue() * Time.deltaTime * 1000f : Vector2.zero;
#else
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
    }
}
