using UnityEngine;

public class CameraRigMouseLookTPS : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;     // Player Transform (회전시킬 대상)
    public Transform pitchPivot;     // CameraPitch
    public Transform cam;            // Main Camera

    [Header("Look settings")]
    public float sensX = 150f;       // horizontal sensitivity
    public float sensY = 120f;       // vertical sensitivity
    public float minPitch = -40f;    // look down limit
    public float maxPitch = 70f;     // look up limit
    public bool lockCursor = true;   // hide & lock cursor

    [Header("Camera distance")]
    public Vector3 offset = new Vector3(0f, 1.6f, -3.5f);  // camera local offset behind player
    public float followLerp = 15f;   // follow smoothing

    float _yaw;                      // horizontal rotation accumulator
    float _pitch;                    // vertical rotation accumulator

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // initialize from current rotation
        _yaw = playerRoot.eulerAngles.y;
        _pitch = pitchPivot.localEulerAngles.x;
        if (_pitch > 180f) _pitch -= 360f;
    }

    void LateUpdate()
    {
        Vector2 look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        UpdateRotation(look, Time.deltaTime);
        UpdateCameraPosition(Time.deltaTime);
    }

    void UpdateRotation(Vector2 look, float dt)
    {
        _yaw += look.x * sensX * dt;
        _pitch -= look.y * sensY * dt;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // apply to player (yaw only)
        if (playerRoot != null)
            playerRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);

        // apply pitch to pivot
        if (pitchPivot != null)
            pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void UpdateCameraPosition(float dt)
    {
        if (pitchPivot == null || cam == null) return;

        // follow player smoothly
        Vector3 desiredPos = playerRoot.position;
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followLerp * dt));

        // set camera offset relative to pivot
        cam.localPosition = offset;
    }
}
