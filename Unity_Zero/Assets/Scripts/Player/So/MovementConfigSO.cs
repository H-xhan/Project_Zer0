using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Config/MovementConfig", fileName = "MovementConfig")]
public class MovementConfigSO : ScriptableObject
{
    [Header("Speed Settings")]
    [Tooltip("걷기 속도")]
    public float walkSpeed = 4f;

    [Tooltip("스프린트 속도 배수")]
    public float sprintMultiplier = 1.7f;

    [Tooltip("스프린트 입력 키")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Tooltip("지상에서만 스프린트 허용 여부")]
    public bool sprintOnlyOnGround = true;

    [Header("Jump Settings")]
    [Tooltip("점프 높이 (미터 단위)")]
    public float jumpHeight = 1.2f;

    [Tooltip("점프 후 다음 점프까지 최소 대기 시간")]
    public float jumpCooldown = 0.2f;

    [Tooltip("코요테 타임 (착지 직후 점프 허용 시간)")]
    public float coyoteTime = 0.1f;

    [Tooltip("점프 버퍼 시간 (입력을 저장하는 시간)")]
    public float jumpBufferTime = 0.1f;

    [Header("Gravity / Ground")]
    [Tooltip("중력 가속도 (음수 권장)")]
    public float gravity = -20f;

    [Tooltip("경사면에서 지면에 붙게 하는 보정 값 (음수 권장)")]
    public float groundedStick = -2f;

    [Header("Sprint Exhaust")]
    [Tooltip("효율 소진 시 스프린트를 즉시 중단할지 여부")]
    public bool stopOnSprintExhaust = true;

    [Tooltip("효율 소진 후 강제 제약 지속 시간")]
    public float exhaustStopDuration = 0.2f;
}