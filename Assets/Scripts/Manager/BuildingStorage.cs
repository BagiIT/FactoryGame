using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
public enum BuildingCategorys
{
    Production,
    Logistics,
}

public enum SubCategory {
    Smelter,
    Miner,
    Processor,
    Belt,
}
public class BuildingStorage : MonoBehaviour
{
    public static BuildingStorage Instance { get; private set; }

    [SerializeField] private List<BuildingData> buildingList = new List<BuildingData>();
    private Dictionary<BuildingCategorys, List<BuildingData>> buildingDictionay = new Dictionary<BuildingCategorys, List<BuildingData>>();
    [SerializeField] private List<GameObject> builtBuildingsList = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        OrganizeBuildingByCategory();
        SaveLoad.OnLoadGame += LoadGame;
    }
    private void OnDestroy()
    {

        SaveLoad.OnLoadGame -= LoadGame;
    }
    private void LoadGame(SaveData arg0)
    {
        foreach (KeyValuePair<string, BuildingSaveData> building in arg0.buildings)
        {
            Debug.Log($"{building.Key} loaded {building.Value}");
            //Instantiate(buildingList[building.Value.BuildingListIndex].BuildingPrefab, building.Value.Position, building.Value.Rotation);
        }
    }
    private void OrganizeBuildingByCategory()
    {
        buildingDictionay.Clear();
        foreach (var building in buildingList)
        {
            if (!buildingDictionay.ContainsKey(building.buildingCategory))
            {
                buildingDictionay[building.buildingCategory] = new List<BuildingData>();
            }

            buildingDictionay[building.buildingCategory].Add(building);
        }
    }
    public BuildingData GetBuilding(BuildingCategorys categorys, int index)
    {
        if (!buildingDictionay.ContainsKey(categorys)) return null;

        List<BuildingData> buildings = buildingDictionay[categorys];

        if (index < 0 || index >= buildings.Count) return null;
        return buildings[index];
    }

    public int GetIndex(BuildingData buildingData)
    {
        return buildingList.FindIndex(x => x == buildingData);
        
    }
}

[System.Serializable]
public struct BuildingSaveData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsProcessing;
    public int CurrentRecepieIndex;

    public BuildingSaveData(List<RecipeData> AvailableRecepies, Vector3 Position, Quaternion Rotation, bool IsProcessing, int CurrentRecepieIndex, int BuildingListIndex)
    {
        this.Position = Position;
        this.Rotation = Rotation;
        this.IsProcessing = IsProcessing;
        this.CurrentRecepieIndex = CurrentRecepieIndex;
    }
}

