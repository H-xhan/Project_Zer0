using UnityEngine;

public enum QuestObjectiveType { ReachZone, Collect }

[CreateAssetMenu(menuName = "TimeEconomy/Quest", fileName = "Q_NewQuest")]
public class QuestSO : ScriptableObject
{
    [Header("표시용")]
    public string questId = "Q001";
    public string Id => questId;   // 호환용 별칭 (원래 코드가 quest.id를 참조해도 OK)
    public string id => questId;

    public string title = "지점 도달";
    [TextArea] public string description = "마커 지점까지 이동하라.";

    [Header("목표")]
    public QuestObjectiveType objectiveType = QuestObjectiveType.ReachZone;
    public string objectiveParam = "Zone_A";
    public int targetCount = 1;

    [Header("보상")]
    public float timeRewardSeconds = 60f;
}
