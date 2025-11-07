using UnityEngine;

public class CameraRigMouseLookTPS : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    [Header("References")]
    [Tooltip("플레이어 루트(수평 회전 기준)")]
    public Transform playerRoot;

    [Tooltip("상하 회전 피벗(카메라 부모 트랜스폼)")]
    public Transform pitchPivot;

    [Tooltip("실제 사용되는 카메라")]
    public Camera cam;

    [Header("Look Settings")]
    [Tooltip("마우스 X 감도")]
    public float sensX = 150f;

    [Tooltip("마우스 Y 감도")]
    public float sensY = 120f;

    [Tooltip("최소 피치 각도")]
    public float minPitch = -45f;

    [Tooltip("최대 피치 각도")]
    public float maxPitch = 75f;

    [Tooltip("시작 시 커서를 잠글지 여부")]
    public bool lockCursor = true;

    [Header("Anchors")]
    [Tooltip("1인칭 기준 위치(머리/눈 위치용)")]
    public Transform firstPersonAnchor;

    [Tooltip("3인칭 기준 위치(캐릭터 뒤쪽 앵커)")]
    public Transform thirdPersonAnchor;

    [Tooltip("1인칭 오프셋 (앵커 기준)")]
    public Vector3 firstPersonOffset = new Vector3(0f, 0.06f, 0.08f);

    [Tooltip("3인칭 오프셋 (앵커 기준)")]
    public Vector3 thirdPersonOffset = Vector3.zero;

    [Tooltip("플레이어를 따라가는 보간 속도")]
    public float followLerp = 15f;

    [Header("Culling (Optional)")]
    [Tooltip("1인칭에서 사용할 Culling Mask (PlayerBody 숨길 때 사용)")]
    public LayerMask firstPersonMask;

    [Tooltip("3인칭에서 사용할 Culling Mask")]
    public LayerMask thirdPersonMask;

    [Header("Start Mode")]
    [Tooltip("시작 시 적용할 카메라 모드")]
    public CameraMode startMode = CameraMode.FirstPerson;

    // 내부 회전 상태
    float _yaw;
    float _pitch;

    // 현재 카메라 모드
    CameraMode _currentMode;
    public CameraMode CurrentMode => _currentMode;

    private void Start()
    {
        // 커서 잠금 설정
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 플레이어 루트 자동 참조
        if (playerRoot == null && transform.parent != null)
            playerRoot = transform.parent;

        // 초기 회전 세팅
        if (playerRoot != null)
            _yaw = playerRoot.eulerAngles.y;

        if (pitchPivot == null)
            pitchPivot = transform;

        _pitch = pitchPivot.localEulerAngles.x;
        if (_pitch > 180f) _pitch -= 360f;

        // 앵커 기본 설정
        if (firstPersonAnchor == null)
            firstPersonAnchor = pitchPivot;
        if (thirdPersonAnchor == null)
            thirdPersonAnchor = pitchPivot;

        // 시작 모드 적용
        SetMode(startMode, true);
    }

    private void LateUpdate()
    {
        if (cam == null || playerRoot == null || pitchPivot == null)
            return;

        // 마우스 입력 읽기
        Vector2 look = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        UpdateRotation(look, Time.deltaTime);
        UpdateCameraPosition(Time.deltaTime);
    }

    // 회전 처리
    void UpdateRotation(Vector2 look, float dt)
    {
        _yaw += look.x * sensX * dt;
        _pitch -= look.y * sensY * dt;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        playerRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
        pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    // 위치 및 모드에 따른 카메라 배치 처리
    void UpdateCameraPosition(float dt)
    {
        // 리그 위치를 플레이어에 보간 이동
        Vector3 rigTargetPos = playerRoot.position;
        transform.position = Vector3.Lerp(
            transform.position,
            rigTargetPos,
            1f - Mathf.Exp(-followLerp * dt)
        );

        // 모드에 따른 앵커와 오프셋 선택
        Transform anchor = _currentMode == CameraMode.FirstPerson
            ? firstPersonAnchor
            : thirdPersonAnchor;

        Vector3 offset = _currentMode == CameraMode.FirstPerson
            ? firstPersonOffset
            : thirdPersonOffset;

        // 카메라 목표 위치
        Vector3 camTargetPos = anchor.TransformPoint(offset);

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            camTargetPos,
            1f - Mathf.Exp(-followLerp * dt)
        );

        cam.transform.rotation = pitchPivot.rotation;
    }

    // 외부에서 호출하는 모드 변경용
    public void SetMode(CameraMode mode)
    {
        SetMode(mode, false);
    }

    // 모드 변경 내부 처리
    void SetMode(CameraMode mode, bool instant)
    {
        _currentMode = mode;

        // 모드별 Culling Mask 변경
        if (cam != null)
        {
            if (mode == CameraMode.FirstPerson && firstPersonMask.value != 0)
                cam.cullingMask = firstPersonMask;
            else if (mode == CameraMode.ThirdPerson && thirdPersonMask.value != 0)
                cam.cullingMask = thirdPersonMask;
        }

        // 즉시 전환 옵션
        if (instant && cam != null)
        {
            Transform anchor = mode == CameraMode.FirstPerson
                ? firstPersonAnchor
                : thirdPersonAnchor;

            Vector3 offset = mode == CameraMode.FirstPerson
                ? firstPersonOffset
                : thirdPersonOffset;

            cam.transform.position = anchor.TransformPoint(offset);
            cam.transform.rotation = pitchPivot.rotation;
        }
    }
}
