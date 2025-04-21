using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "SO/BuildingData", order = 1)]
public class BuildingData : ScriptableObject
{
    public int ID = -1;
    public string Name;
    public int inputPort;
    public int outputPort;
    public BuildingCategorys buildingCategory;
    public SubCategory subCategory;
    public GameObject BuildingPrefab;
    public GameObject hologramPrefab;

    public List<RecipeData> AvailableRecepies;

    public List<BuildCost> BuildingCost;
   
    public BuildingData(BuildingData data)
    {
        this.ID = data.ID;
        this.Name = data.name;
        this.inputPort = data.inputPort;
        this.outputPort = data.outputPort;
        this.buildingCategory = data.buildingCategory;
        this.BuildingPrefab = data.BuildingPrefab;
        this.hologramPrefab = data.hologramPrefab;
        this.AvailableRecepies = data.AvailableRecepies;
        this.BuildingCost = data.BuildingCost;
    }
}

[Serializable]
public class BuildCost
{
    public ItemData item;
    public int amount;
}
