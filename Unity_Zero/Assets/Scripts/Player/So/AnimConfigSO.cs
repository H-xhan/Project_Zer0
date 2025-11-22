using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Config/AnimConfig", fileName = "AnimConfig")]
public class AnimConfigSO : ScriptableObject
{
    [Header("Speed Damp")]
    [Tooltip("속도 증가 시 애니메이터 반응 속도")]
    public float dampUp = 0.08f;

    [Tooltip("속도 감소 시 애니메이터 반응 속도")]
    public float dampDown = 0.04f;

    [Header("Stop Threshold")]
    [Tooltip("이동 속도가 이 값 이하이면 정지로 처리")]
    public float stopSnapThreshold = 0.08f;
}
