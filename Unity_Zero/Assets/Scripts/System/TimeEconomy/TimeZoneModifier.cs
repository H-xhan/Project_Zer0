using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TimeZoneModifier : MonoBehaviour
{
    [Tooltip("이 구역에 있을 동안 TimeManager에 적용될 배율")]
    public float zoneMultiplier = 2f; // 위험구역이면 2~3, 안전구역이면 0.5 등

    private void OnTriggerEnter(Collider other)
    {
        var mgr = other.GetComponentInParent<TimeManager>();
        if (!mgr) mgr = FindFirstObjectByType<TimeManager>();
        if (mgr) mgr.SetZoneMultiplier(zoneMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        var mgr = other.GetComponentInParent<TimeManager>();
        if (!mgr) mgr = FindFirstObjectByType<TimeManager>();
        if (mgr) mgr.SetZoneMultiplier(1f);
    }
}
