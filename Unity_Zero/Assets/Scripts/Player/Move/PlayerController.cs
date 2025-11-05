using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Modules")]
    public MovementModule movement;
    public PlayerAnimModule animModule;

    [Header("Animation Ref")]
    public Animator animatorSource; // assign model Animator (auto find if null)

    CharacterController _cc;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (movement != null)
            movement.Initialize(_cc, transform);

        if (animatorSource == null)
            animatorSource = GetComponentInChildren<Animator>(true);

        if (animModule != null)
            animModule.Init(animatorSource);
    }

    void Update()
    {
        if (movement != null)
            movement.Tick();

        if (animModule != null)
        {
            float planar = movement.GetPlanarSpeed();
            bool grounded = movement.IsGrounded();
            float vY = movement.GetVerticalVelocity();
            bool jumpTrig = movement.ConsumeJumpTriggered();
            bool sprinting = movement.IsSprinting(); // 추가

            animModule.Tick(Time.deltaTime, planar, grounded, vY, jumpTrig, sprinting);
        }
    }
}
