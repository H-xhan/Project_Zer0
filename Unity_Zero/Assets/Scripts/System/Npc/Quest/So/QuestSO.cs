using UnityEngine;

[CreateAssetMenu(fileName = "Q_NewQuest", menuName = "ProjectZer0/Quest", order = 0)]
public class QuestSO : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;                               // 퀘스트 고유 ID
    public string title;                                 // 퀘스트 제목
    [TextArea] public string description;                // 퀘스트 설명

    [Header("플래그")]
    public bool isTimed;                                 // 시간 제한 퀘스트인지
    public bool isDangerQuest;                          // 위험/전투성 퀘스트인지
}
