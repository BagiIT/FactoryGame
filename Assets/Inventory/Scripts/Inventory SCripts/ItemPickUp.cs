using System;
using UnityEngine;


[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(SphereCollider))]
public class ItemPickUp : MonoBehaviour
{
    public float pickUpRadius = 1f;
    public ItemData itemData;
    [SerializeField] private float rotationSpeed;

    private SphereCollider collider;

    

    private string id;
    [SerializeField] private ItemPickUpSaveData itemSaveData;
   

    private void Awake()
    {
        
        SaveLoad.OnLoadGame += LoadGame;
        itemSaveData = new ItemPickUpSaveData(itemData, transform.position,transform.rotation);

        collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = pickUpRadius;
    }
    private void OnDestroy()
    {
        if (SaveGameManager.data.activeItems.ContainsKey(id)) SaveGameManager.data.activeItems.Remove(id);
        SaveLoad.OnLoadGame -= LoadGame;
    }

    private void Start()
    {
        id = GetComponent<UniqueID>().ID;
        SaveGameManager.data.activeItems.Add(id, itemSaveData);
    }
    private void LoadGame(SaveData arg0)
    {
        if (arg0.collectedItems.Contains(id)) Destroy(this.gameObject);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();
        if (!inventory) return;

        if (inventory.AddToInventory(itemData, 1))
        {
            SaveGameManager.data.collectedItems.Add(id);
            Destroy(this.gameObject);
        }
    }

    
}

[System.Serializable]
public struct ItemPickUpSaveData
{
    public ItemData ItemData;
    public Vector3 position;
    public Quaternion rotation;
    public ItemPickUpSaveData(ItemData data, Vector3 position, Quaternion rotation)
    {
        ItemData = data;
        this.position = position;
        this.rotation = rotation;
    }
}