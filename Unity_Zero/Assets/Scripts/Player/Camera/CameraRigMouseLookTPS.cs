using UnityEngine;

public class CameraRigMouseLookTPS : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    [Header("References")]
    public Transform playerRoot;          // 플레이어 루트 (Yaw)
    public Transform pitchPivot;          // 상하 회전 피벗 (카메라 부모)
    public Camera cam;                    // 실제 카메라

    [Header("Look settings")]
    public float sensX = 150f;
    public float sensY = 120f;
    public float minPitch = -45f;         // 너무 아래로 못 보게 (가슴 안보이게)
    public float maxPitch = 75f;
    public bool lockCursor = true;

    [Header("Anchors")]
    public Transform firstPersonAnchor;   // 1인칭 기준 (머리/눈 위치 빈 오브젝트)
    public Transform thirdPersonAnchor;   // 3인칭 기준 (캐릭터 뒤 빈 오브젝트)
    public Vector3 firstPersonOffset = new Vector3(0f, 0.06f, 0.08f);
    // 살짝 위/앞: 머리 밖으로 내밀기
    public Vector3 thirdPersonOffset = new Vector3(0f, 0f, 0f);
    public float followLerp = 15f;

    [Header("Culling (Optional)")]
    // 1인칭일 때 플레이어 몸(특히 머리)을 안 보고 싶으면 세팅
    public LayerMask firstPersonMask;     // ex: Everything & ~PlayerBody
    public LayerMask thirdPersonMask;     // ex: Everything

    [Header("Debug")]
    public CameraMode startMode = CameraMode.FirstPerson;

    float _yaw;
    float _pitch;
    CameraMode _currentMode;

    public CameraMode CurrentMode => _currentMode;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerRoot == null && transform.parent != null)
            playerRoot = transform.parent;

        // 초기 회전
        if (playerRoot != null)
            _yaw = playerRoot.eulerAngles.y;

        if (pitchPivot == null)
            pitchPivot = transform; // 안전망

        _pitch = pitchPivot.localEulerAngles.x;
        if (_pitch > 180f) _pitch -= 360f;

        // 앵커 없으면 기본값: 1인칭 = pitchPivot 위치, 3인칭 = pitchPivot 뒤쪽
        if (firstPersonAnchor == null)
            firstPersonAnchor = pitchPivot;
        if (thirdPersonAnchor == null)
            thirdPersonAnchor = pitchPivot;

        SetMode(startMode, true);
    }

    void LateUpdate()
    {
        if (cam == null || playerRoot == null || pitchPivot == null)
            return;

        Vector2 look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        UpdateRotation(look, Time.deltaTime);
        UpdateCameraPosition(Time.deltaTime);
    }

    void UpdateRotation(Vector2 look, float dt)
    {
        _yaw += look.x * sensX * dt;
        _pitch -= look.y * sensY * dt;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        playerRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
        pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void UpdateCameraPosition(float dt)
    {
        // rig(pitchPivot 부모)를 플레이어 위치로 부드럽게 이동
        Vector3 rigTargetPos = playerRoot.position;
        transform.position = Vector3.Lerp(
            transform.position,
            rigTargetPos,
            1f - Mathf.Exp(-followLerp * dt)
        );

        // 모드별 타겟 위치 계산
        Transform anchor = _currentMode == CameraMode.FirstPerson
            ? firstPersonAnchor
            : thirdPersonAnchor;

        Vector3 offset = _currentMode == CameraMode.FirstPerson
            ? firstPersonOffset
            : thirdPersonOffset;

        // 앵커 기준 위치
        Vector3 camTargetPos = anchor.TransformPoint(offset);

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            camTargetPos,
            1f - Mathf.Exp(-followLerp * dt)
        );

        cam.transform.rotation = pitchPivot.rotation;
    }

    public void SetMode(CameraMode mode)
    {
        SetMode(mode, false);
    }

    void SetMode(CameraMode mode, bool instant)
    {
        _currentMode = mode;

        // 선택: 모드 전환 시 cullingMask 변경해서 1인칭에서 몸 숨기기
        if (cam != null)
        {
            if (mode == CameraMode.FirstPerson && firstPersonMask.value != 0)
                cam.cullingMask = firstPersonMask;
            else if (mode == CameraMode.ThirdPerson && thirdPersonMask.value != 0)
                cam.cullingMask = thirdPersonMask;
        }

        if (instant)
        {
            // 즉시 위치 동기화
            Transform anchor = mode == CameraMode.FirstPerson ? firstPersonAnchor : thirdPersonAnchor;
            Vector3 offset = mode == CameraMode.FirstPerson ? firstPersonOffset : thirdPersonOffset;

            cam.transform.position = anchor.TransformPoint(offset);
            cam.transform.rotation = pitchPivot.rotation;
        }
    }
}
