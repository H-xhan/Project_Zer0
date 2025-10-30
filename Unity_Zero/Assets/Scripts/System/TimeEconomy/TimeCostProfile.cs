using UnityEngine;

[CreateAssetMenu(menuName = "ProjectZer0/Time Economy/TimeCostProfile")]
public class TimeCostProfile : ScriptableObject
{
    [Header("Move & Sprint")]
    public float moveCostPerMeter = 0.1f;     // 이동 1m당
    public float sprintExtraPerSecond = 0.2f; // 스프린트 중 초당

    [Header("Actions")]
    public float jumpCost = 0.5f;
    public float interactCost = 0.2f;         // 상호작용 키
    public float attackCost = 0.5f;           // 공격 1회
    public float inventoryOpenCost = 0.1f;    // 인벤토리 열기 등

    [Header("Fallback to Config?")]
    public bool useConfigFallback = true;     // 비어있을 때 TimeConfig 값 사용
}
