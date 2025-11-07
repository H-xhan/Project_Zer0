using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickupTarget : MonoBehaviour
{
    [Tooltip("플레이어가 획득할 아이템")]
    public ItemSO item;

    [Tooltip("획득 시 지급할 개수")]
    public int amount = 1;

    private void Awake()
    {
        // 픽업 대상은 트리거 콜라이더로 처리
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
