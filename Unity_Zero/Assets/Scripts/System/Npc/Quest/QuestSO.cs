using UnityEngine;

public enum QuestObjectiveType { ReachZone /*(지점 도달)*/, Collect /*(수집: 선택)*/ }

[CreateAssetMenu(menuName = "TimeEconomy/Quest", fileName = "Q_NewQuest")]
public class QuestSO : ScriptableObject
{
    [Header("표시용")]
    public string questId = "Q001";           // 고유 아이디
    public string title = "지점 도달";
    [TextArea] public string description = "마커 지점까지 이동하라.";

    [Header("목표")]
    public QuestObjectiveType objectiveType = QuestObjectiveType.ReachZone;
    public string objectiveParam = "Zone_A";  // 도달해야 할 Zone 이름(또는 수집 아이템 키)
    public int targetCount = 1;               // 필요 개수(도달형은 1)

    [Header("보상")]
    public float timeRewardSeconds = 60f;     // 보상: 시간(초)
}
