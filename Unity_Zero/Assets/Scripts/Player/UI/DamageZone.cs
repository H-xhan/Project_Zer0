using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [Tooltip("초당 혹은 단발로 적용할 피해량")]
    public float dps = 10f;

    [Tooltip("영역 안에 있는 동안 지속 피해면 활성, 진입 시 1회 피해면 비활성")]
    public bool onlyWhileInside = true;

    private void Awake()
    {
        // 데미지 존은 트리거 콜라이더로 동작
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        // 체류 중 지속 피해
        if (!onlyWhileInside)
            return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
            pc.ApplyDamage(dps * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 진입 시 1회 피해
        if (onlyWhileInside)
            return;

        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
            pc.ApplyDamage(dps);
    }
}
