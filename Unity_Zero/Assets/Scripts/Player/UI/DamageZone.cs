using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    public float dps = 10f; // per second damage
    public bool onlyWhileInside = true;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (!onlyWhileInside) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.ApplyDamage(dps * Time.deltaTime);
        }
    }

    // 키 입력으로 한 번에 때려보기 원하면 아래 메서드 임시 사용
    void OnTriggerEnter(Collider other)
    {
        if (onlyWhileInside) return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.ApplyDamage(dps);
        }
    }
}
