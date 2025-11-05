using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;          // unique id
    public string displayName;     // ui name
    public Sprite icon;            // optional
    public bool stackable = true;
    public int maxStack = 99;
}
