using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // -------- Move / Jump --------
    public float moveSpeed = 4.0f;
    public float jumpHeight = 1.2f;
    public float gravity = -20.0f;
    public float groundedStick = -2.0f;
    public Transform moveReference;       // optional camera yaw

    // -------- Animation bridge --------
    public PlayerAnim anim;   // drag the driver here

    // -------- Private --------
    CharacterController _cc;
    Vector3 _input;
    Vector3 _horizontal;
    float _vertical;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        ReadInput();
        MoveHorizontal();

        bool jumpTriggered = JumpAndGravity();   // returns true only on pressed this frame

        ApplyMovement();

        // drive animation via the driver
        if (anim != null)
        {
            float planarSpeed = new Vector3(_horizontal.x, 0f, _horizontal.z).magnitude;
            bool groundedNow = _cc.isGrounded;
            anim.Tick(Time.deltaTime, planarSpeed, groundedNow, _vertical, jumpTriggered);
        }
    }

    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _input = new Vector3(h, 0f, v);
        _input = Vector3.ClampMagnitude(_input, 1f);
    }

    void MoveHorizontal()
    {
        // move relative to player forward/right, not camera
        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();

        Vector3 dir = fwd * _input.z + right * _input.x;
        _horizontal = dir * moveSpeed;
    }

    // returns true only if jump was triggered this frame
    bool JumpAndGravity()
    {
        bool grounded = _cc.isGrounded;
        bool jumpTriggered = false;

        if (grounded)
        {
            if (_vertical < groundedStick) _vertical = groundedStick;

            if (Input.GetButtonDown("Jump"))
            {
                float g = Mathf.Abs(gravity);
                _vertical = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
                jumpTriggered = true;
            }
        }
        else
        {
            _vertical += gravity * Time.deltaTime;
        }

        return jumpTriggered;
    }

    void ApplyMovement()
    {
        Vector3 velocity = new Vector3(_horizontal.x, _vertical, _horizontal.z);
        _cc.Move(velocity * Time.deltaTime);
    }
}
