using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]
public class ChestInventory : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Start()
    {
        var chestSaveData = new InventorySaveData(PrimaryInventorySystem,transform.position,transform.rotation);

        SaveGameManager.data.chestDictionary.Add(GetComponent<UniqueID>().ID, chestSaveData);
        //SaveGameManager.Debug();
    }


    protected override void LoadInventory(SaveData arg0)
    {
        //check save data for this chest inv if exits loads it
        Debug.Log(arg0);
        if (arg0.chestDictionary.TryGetValue(GetComponent<UniqueID>().ID, out InventorySaveData chestData))
        {
            this.primaryInventorySystem = chestData.inventorySystem;
            this.transform.position = chestData.position;
            this.transform.rotation = chestData.rotation;
        }
    }

    public void Interact(Interactor interactor, out bool interactionSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem,0);
        PlayerInventoryHolder.Instance.OpenPlayerInventory();
        interactionSuccessful = true;
    }
    public void EndInteraction()
    {

    }
}

