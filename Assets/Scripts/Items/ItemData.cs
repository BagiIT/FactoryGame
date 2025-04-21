using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "SO/Items/Inventory", order = 1)]
public class ItemData : ScriptableObject
{
    public int ID = -1;
    public string itemName;
    public Sprite uiIcon;
    public Sprite uiIconOutline;
    public bool isStackable = true;
    public int maxStack = 200;
    public GameObject ItemPrefab;
    public Mesh beltMesh;

    public void UseItem()
    {
        Debug.Log($"Using {itemName}");  
    }
}
