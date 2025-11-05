using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [Header("Animator")]
    public Animator anim;                   // assign your model animator
    public string paramSpeed = "Speed";     // float
    public string paramJump = "Jump";       // trigger
    public string paramIsFalling = "IsFalling"; // bool
    public string paramLand = "Land";       // trigger

    [Header("Tuning")]
    public float speedLerp = 10f;           // smoothing for Speed
    public float fallingThreshold = -0.1f;  // vertical < this means falling

    float _smoothedSpeed;
    bool _wasGrounded;

    // Call this once per frame from PlayerController
    public void Tick(float dt, float planarSpeed, bool grounded, float vertical, bool jumpTriggered)
    {
        if (anim == null) return;

        // speed (float)
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, planarSpeed, speedLerp * dt);
        if (!string.IsNullOrEmpty(paramSpeed))
            anim.SetFloat(paramSpeed, _smoothedSpeed);

        // falling (bool)
        bool isFalling = !grounded && vertical < fallingThreshold;
        if (!string.IsNullOrEmpty(paramIsFalling))
            anim.SetBool(paramIsFalling, isFalling);

        // jump (trigger)
        if (jumpTriggered && !string.IsNullOrEmpty(paramJump))
            anim.SetTrigger(paramJump);

        // land (trigger) edge: air -> ground
        if (!_wasGrounded && grounded)
        {
            if (!string.IsNullOrEmpty(paramLand))
                anim.SetTrigger(paramLand);
        }

        _wasGrounded = grounded;
    }
}
