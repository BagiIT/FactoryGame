using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveData data;

    private void Awake()
    {
        data = new SaveData();
        SaveLoad.OnLoadGame += LoadData;
    }

    public static void Debug()
    {
        foreach (KeyValuePair<string, InventorySaveData> items in data.chestDictionary)
        {
            print("You have " + items.Value.inventorySystem + " " + items.Key);

        }
    }

    public static void LoadData(SaveData arg0)
    {
        data = arg0;
    }
    public static void SaveData()
    {
        var saveData = data;

        SaveLoad.Save(saveData);
    }

    public void DeleteData()
    {
        SaveLoad.DeleteSaveData();
    }

    public static void TryLoadData()
    {
        SaveLoad.Load();
    }
}
