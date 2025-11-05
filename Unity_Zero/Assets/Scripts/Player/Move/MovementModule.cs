using UnityEngine;

[System.Serializable]
public class MovementModule
{
    [Header("Movement")]
    public float walkSpeed = 4.0f;
    public float sprintMultiplier = 1.7f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool sprintOnlyOnGround = true;

    [Header("Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -20.0f;
    public float groundedStick = -2.0f;

    // internal state
    private CharacterController _cc;
    private Transform _transform;
    private Vector3 _input;
    private Vector3 _horizontal;
    private float _vertical;
    private bool _isSprinting;
    private bool _jumpTriggered; // this frame only

    public void Initialize(CharacterController cc, Transform t)
    {
        _cc = cc;
        _transform = t;
    }

    public void Tick()
    {
        if (_cc == null || _transform == null) return;

        ReadInput();
        UpdateSprintState();
        MoveHorizontal();
        JumpAndGravity();
        ApplyMovement();
    }

    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _input = new Vector3(h, 0f, v);
        _input = Vector3.ClampMagnitude(_input, 1f);
    }

    void UpdateSprintState()
    {
        bool wantSprint = Input.GetKey(sprintKey);
        bool grounded = _cc.isGrounded;

        if (sprintOnlyOnGround)
            _isSprinting = wantSprint && grounded && _input.sqrMagnitude > 0.001f;
        else
            _isSprinting = wantSprint && _input.sqrMagnitude > 0.001f;
    }

    void MoveHorizontal()
    {
        Vector3 fwd = _transform.forward;
        Vector3 right = _transform.right;
        fwd.y = 0f; right.y = 0f;
        fwd.Normalize(); right.Normalize();

        Vector3 dir = (fwd * _input.z) + (right * _input.x);
        dir = Vector3.ClampMagnitude(dir, 1f);

        float speed = walkSpeed * (_isSprinting ? sprintMultiplier : 1f);
        _horizontal = dir * speed;
    }

    void JumpAndGravity()
    {
        bool grounded = _cc.isGrounded;
        _jumpTriggered = false; // reset each frame

        if (grounded)
        {
            if (_vertical < groundedStick) _vertical = groundedStick;

            if (Input.GetButtonDown("Jump"))
            {
                float g = Mathf.Abs(gravity);
                _vertical = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
                _jumpTriggered = true;
            }
        }
        else
        {
            _vertical += gravity * Time.deltaTime;
        }
    }

    void ApplyMovement()
    {
        Vector3 velocity = new Vector3(_horizontal.x, _vertical, _horizontal.z);
        _cc.Move(velocity * Time.deltaTime);
    }

    // getters for other modules
    public bool IsSprinting() { return _isSprinting; }
    public bool IsGrounded() { return _cc != null && _cc.isGrounded; }
    public float GetPlanarSpeed() { return _horizontal.magnitude; }
    public Vector3 GetMoveDirection() { return _horizontal.normalized; }
    public float GetVerticalVelocity() { return _vertical; }
    public bool ConsumeJumpTriggered()
    {
        bool v = _jumpTriggered;
        _jumpTriggered = false;
        return v;
    }
}
