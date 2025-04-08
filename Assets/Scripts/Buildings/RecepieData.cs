using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Recepie", menuName = "SO/Recepie")]
public class RecipeData : ScriptableObject
{
    [Serializable]
    public class Ingredient
    {
        public ItemData item;
        public int amount;
    }

    public string name;
    public List<Ingredient> inputItems = new List<Ingredient>();
    public ItemData outputItem;
    public int outputAmount = 1;
    public float processingTime = 5f;
    //50 = 1S IF 0,02F
}
