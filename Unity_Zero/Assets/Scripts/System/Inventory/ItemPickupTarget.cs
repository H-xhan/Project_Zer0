using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickupTarget : MonoBehaviour
{
    public ItemSO item;
    public int amount = 1;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
