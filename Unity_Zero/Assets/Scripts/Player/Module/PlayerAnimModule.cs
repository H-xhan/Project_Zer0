using UnityEngine;

[System.Serializable]
public class PlayerAnimModule
{
    [Header("Animator")]
    public Animator anim;
    public string paramSpeed = "Speed";         // float
    public string paramJump = "Jump";           // trigger
    public string paramIsFalling = "IsFalling"; // bool
    public string paramLand = "Land";           // trigger
    public string paramIsSprinting = "IsSprinting"; // bool (optional)

    [Header("Tuning")]
    public float speedLerp = 10f;
    public float fallingThreshold = -0.1f;

    [Header("Speed scale")]
    public float speedToParam = 0.25f; // planarSpeed * speedToParam -> 0..1

    [Header("Debug")]
    public bool logOnceIfAnimatorNull = true;
    bool _logged;

    float _smoothedSpeed;
    bool _wasGrounded;

    public void Init(Animator a)
    {
        anim = a;
    }

    public void Tick(float dt, float planarSpeed, bool grounded, float vertical, bool jumpTriggered, bool isSprinting)
    {
        if (anim == null)
        {
            if (logOnceIfAnimatorNull && !_logged)
            {
                Debug.Log("[PlayerAnimModule] Animator is null. Assign animatorSource on PlayerController.");
                _logged = true;
            }
            return;
        }

        // Speed
        float param = planarSpeed * speedToParam;
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, param, speedLerp * dt);
        if (!string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, _smoothedSpeed);

        // Falling
        bool isFalling = !grounded && vertical < fallingThreshold;
        if (!string.IsNullOrEmpty(paramIsFalling))
            anim.SetBool(paramIsFalling, isFalling);

        // Jump
        if (jumpTriggered && !string.IsNullOrEmpty(paramJump))
            anim.SetTrigger(paramJump);

        // Land
        if (!_wasGrounded && grounded)
        {
            if (!string.IsNullOrEmpty(paramLand))
                anim.SetTrigger(paramLand);
        }

        // Sprint
        if (!string.IsNullOrEmpty(paramIsSprinting))
            anim.SetBool(paramIsSprinting, isSprinting);

        _wasGrounded = grounded;
    }
}
