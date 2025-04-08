using System.Collections.Generic;
using UnityEngine;

public class SaveData
{
    public List<string> collectedItems;
    public SerializableDictionary<string, InventorySaveData> chestDictionary;

    public SerializableDictionary<string, ItemPickUpSaveData> activeItems;
    public InventorySaveData playerInventory;

    public SerializableDictionary<string, BuildingSaveData> buildings;

    public SaveData()
    {
        chestDictionary = new SerializableDictionary<string, InventorySaveData>();
        collectedItems = new List<string>();
        activeItems = new SerializableDictionary<string, ItemPickUpSaveData>();
        playerInventory = new InventorySaveData();
        buildings = new SerializableDictionary<string, BuildingSaveData>();
    }
    
}
