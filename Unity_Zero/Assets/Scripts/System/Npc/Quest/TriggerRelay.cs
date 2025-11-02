using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    public QuestGiver target; // 부모의 QuestGiver 참조

    void OnTriggerEnter(Collider other)
    {
        target?.HandleTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        target?.HandleTriggerExit(other);
    }
}
