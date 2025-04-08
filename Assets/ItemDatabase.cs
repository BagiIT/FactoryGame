using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;


[CreateAssetMenu(fileName = "SO/Database/Item Database")]
public class ItemDatabase : Database
{

    [SerializeField] private List<ItemData> itemDatabase;

    public ItemData GetItem(int id)
    {
        return itemDatabase.Find(i => i.ID == id);
    }
    public ItemData GetItem(List<ItemData> itemDatabase, string displayName)
    {
        return itemDatabase.Find(i => i.name == displayName);
    }
    [ContextMenu("SetIDs")]
    public override void SetID()
    {
        itemDatabase = new List<ItemData>();

        var foundItems = Resources.LoadAll<ItemData>("ItemData").OrderBy(i => i.ID).ToList();

        var hasIdInRange = foundItems.Where(i => i.ID != -1 && i.ID < foundItems.Count).OrderBy(i => i.ID).ToList();
        var hasIdNotInRange = foundItems.Where(i => i.ID != -1 && i.ID >= foundItems.Count).OrderBy(i => i.ID).ToList();
        var noId = foundItems.Where(i => i.ID <= -1).ToList();

        var tempIndex = 0;
        for (int i = 0; i < foundItems.Count; i++)
        {

            ItemData itemToAdd;
            itemToAdd = hasIdInRange.Find(d => d.ID == i);
            if (itemToAdd != null)
            {
                itemDatabase.Add(itemToAdd);
            }
            else if (tempIndex < noId.Count)
            {
                noId[tempIndex].ID = i;
                itemToAdd = noId[tempIndex];
                tempIndex++;
                itemDatabase.Add(itemToAdd);
            }
#if UNITY_EDITOR
            if (itemToAdd) EditorUtility.SetDirty(itemToAdd);
#endif
        }

        foreach (var item in hasIdNotInRange)
        {
            itemDatabase.Add(item);
#if UNITY_EDITOR
            if (item) EditorUtility.SetDirty(item);
#endif
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }
    

}
