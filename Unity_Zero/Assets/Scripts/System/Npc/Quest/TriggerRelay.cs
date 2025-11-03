using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    public QuestGiver target;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TriggerRelay] Enter by {other.name}");
        if (target) target.HandleTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)   // ← 프레임 놓침 방지
    {
        if (target) target.HandleTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TriggerRelay] Exit by {other.name}");
        if (target) target.HandleTriggerExit(other);
    }
}
