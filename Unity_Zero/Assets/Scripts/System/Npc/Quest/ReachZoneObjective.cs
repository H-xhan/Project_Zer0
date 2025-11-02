using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReachZoneObjective : MonoBehaviour
{
    public string zoneName = "Zone_A";   // QuestSO.objectiveParam와 일치해야 함

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>())
        {
            var tracker = FindFirstObjectByType<QuestTracker>();
            tracker?.NotifyReachZone(zoneName);
            Debug.Log($"📍 [목표] 지점 도달: {zoneName}");
        }
    }
}
