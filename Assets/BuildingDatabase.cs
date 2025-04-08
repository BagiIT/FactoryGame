using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SO/Database/Building Database")]
public class BuildingDatabase : Database
{
    [SerializeField] private List<BuildingData> buildingDatabase;


    public BuildingData GetBuilding(int id)
    {
        return buildingDatabase.Find(i => i.ID == id);
    }
    public BuildingData GetBuilding(List<BuildingData> itemDatabase, string displayName)
    {
        return itemDatabase.Find(i => i.name == displayName);
    }

    [ContextMenu("SetIDs")]
    public override void SetID()
    {
        buildingDatabase = new List<BuildingData>();

        var foundItems = Resources.LoadAll<BuildingData>("BuildingData").OrderBy(i => i.ID).ToList();

        var hasIdInRange = foundItems.Where(i => i.ID != -1 && i.ID < foundItems.Count).OrderBy(i => i.ID).ToList();
        var hasIdNotInRange = foundItems.Where(i => i.ID != -1 && i.ID >= foundItems.Count).OrderBy(i => i.ID).ToList();
        var noId = foundItems.Where(i => i.ID <= -1).ToList();

        var tempIndex = 0;
        for (int i = 0; i < foundItems.Count; i++)
        {

            BuildingData itemToAdd;
            itemToAdd = hasIdInRange.Find(d => d.ID == i);
            if (itemToAdd != null)
            {
                buildingDatabase.Add(itemToAdd);
            }
            else if (tempIndex < noId.Count)
            {
                noId[tempIndex].ID = i;
                itemToAdd = noId[tempIndex];
                tempIndex++;
                buildingDatabase.Add(itemToAdd);
            }
#if UNITY_EDITOR
            if (itemToAdd) EditorUtility.SetDirty(itemToAdd);
#endif
        }

        foreach (var item in hasIdNotInRange)
        {
            buildingDatabase.Add(item);
#if UNITY_EDITOR
            if (item) EditorUtility.SetDirty(item);
#endif
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }
}
