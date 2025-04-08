using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Factory
{
    [ShowInInspector,ReadOnly]
    private List<FactorySystem> factorySystems;
    
    [ShowInInspector,ReadOnly]
    private List<Belt> belts;

    public Factory()
    {
        factorySystems = new List<FactorySystem>();
        belts = new List<Belt>();
        Debug.Log("Help");
    }
    

    public void AddFactory(FactorySystem factoryToAdd) {
        factorySystems.Add(factoryToAdd);
        Debug.Log("addf");
    }

    public void RemoveFactory(FactorySystem factoryToRemove)
    {
        factorySystems.Remove(factoryToRemove);
        Debug.Log("removef");
    }

    public void ClearFactory() {
        foreach (FactorySystem factorySystem in factorySystems) {
            factorySystem.CleanUp();
        }
        factorySystems.Clear(); 
    }

    public void AddBelt(Belt beltToAdd) {
        belts.Add(beltToAdd);
    }
}
