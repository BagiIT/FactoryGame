using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BuildingInventory : InventorySystem
{
    [SerializeField]private List<InventorySlot> inputSlot;
    public List<InventorySlot> InputSlot => inputSlot;
    [SerializeField] private List<InventorySlot> outputSlot;
    public List<InventorySlot> OutputSlot => outputSlot;
    private BuildingData buildingData;

    public BuildingInventory(BuildingData data) : base(data)
    {
        inputSlot = new List<InventorySlot>();
        outputSlot = new List<InventorySlot>();
        inputSlot.Clear();
        outputSlot.Clear();

        for (int i = 0; i < data.inputPort; i++)
        {
            inputSlot.Add(new InventorySlot());
        }
        for (int i = 0; i < data.outputPort; i++)
        {
            outputSlot.Add(new InventorySlot());
        }
    }


    public bool AddToInput(ItemData data, int amount,int slotIndex)
    {
        var freeSlot = inputSlot.FirstOrDefault(i => i.Data == null);
        if (freeSlot != null) {
            freeSlot.UpdateInventorySlot(data, amount);
        }
        else {
            var slot =  inputSlot.Find(i => i.Data == data);
            if (slot.RoomLeftInStack(amount)) {
                slot.AddToStack(amount);
                OnInventorySlotChanged?.Invoke(slot);
                return true;
            }

        }
        OnInventorySlotChanged?.Invoke(inputSlot[slotIndex]);
        return false;
    }
    public bool RemoveFromInput(ItemData data, int amount,int slotIndex)
    {
        var slot = inputSlot.Find(i => i.Data == data);
        if (slot != null) {
            int removeAmount = Mathf.Min(slot.StackSize, amount);
            slot.RemoveFromStack(removeAmount);
            amount -= removeAmount;

            OnInventorySlotChanged?.Invoke(slot);

            if(slot.StackSize <= 0)
            {
                slot.ClearSlot();
                OnInventorySlotChanged?.Invoke(slot);
            }
        }
        if (inputSlot[slotIndex].StackSize <= 0) {
            inputSlot[slotIndex].ClearSlot();
            return true;
        }
        return false;
    }

    public bool AddToOutput(ItemData data, int amount,int slotIndex)
    {
        var freeSlot = outputSlot.FirstOrDefault(i => i.Data == null);
        if (freeSlot != null) {
            freeSlot.UpdateInventorySlot(data, amount);
        }
        else {
          var slot =  outputSlot.Find(i => i.Data == data);
          if (slot.RoomLeftInStack(amount)) {
              slot.AddToStack(amount);
              OnInventorySlotChanged?.Invoke(slot);
              return true;
          }

        }
        OnInventorySlotChanged?.Invoke(outputSlot[slotIndex]);
        return false;
    }
    public bool RemoveFromOutput(ItemData data, int amount,int slotIndex)
    {
        var slot = outputSlot.Find(i => i.Data == data);
        if (slot != null) {
            int removeAmount = Mathf.Min(slot.StackSize, amount);
            slot.RemoveFromStack(removeAmount);
            amount -= removeAmount;

            OnInventorySlotChanged?.Invoke(slot);

            if(slot.StackSize <= 0)
            {
                slot.ClearSlot();
                OnInventorySlotChanged?.Invoke(slot);
            }
        }
        if (outputSlot[slotIndex].StackSize <= 0) {
            outputSlot[slotIndex].ClearSlot();
            return true;
        }
        return false;
    }

    public bool HasSpaceInSlot(ItemData data, InventorySlot slot) {
        if (slot != null) {
            if (slot.StackSize <= data.maxStack) {
                return true;
            }
            else {
                return false;
            }
        }
        Debug.Log("given slot is null");
        return false;
    }

    public List<InventorySlot> GetInputs()
    {
        return inputSlot;
    }
    public List<InventorySlot> GetOutputs()
    {
        return outputSlot;
    }

    public bool HasEnoughItems(RecipeData recipe) {
        /*bool hasEnough = false;
        for (int i = 0; i < recipe.inputItems.Count; i++) {
            for (int j = 0; j < inputSlot.Count; j++) {
                if (recipe.inputItems[i].item == inputSlot[j].Data) {
                    if (inputSlot[j].StackSize >= recipe.inputItems[i].amount) {
                        hasEnough = true;
                    }
                    else {
                        hasEnough = false;
                        return false;
                    }
                }
            }
        }
        return hasEnough;*/
        foreach (var requiredItem in recipe.inputItems) {
            bool found = false;
            foreach (var slot in inputSlot) {
                if (slot.Data == requiredItem.item && slot.StackSize >= requiredItem.amount) {
                    found = true;
                    break; // No need to check further for this item
                }
            }
            if (!found) return false; // If any item is missing, return false immediately
        }
        return true;
    }

    public bool HasOutpuSpace(RecipeData recipe) {
        
        var slot = outputSlot.Find(i => i.Data == null || (i.Data == recipe.outputItem && (i.StackSize + recipe.outputAmount) <= i.Data.maxStack));
        return slot != null;
    }

    public void SetSlotFilters(RecipeData data) {
        /*if(data == null)
            throw new System.ArgumentNullException("data");*/
        for (int i = 0; i < inputSlot.Count; i++) {
            inputSlot[i].AssignFilter(1,data.inputItems[i].item);
        }
        for (int i = 0; i < outputSlot.Count; i++) {
            outputSlot[i].AssignFilter(1,data.outputItem);
        }
    }

    public void ClearSlots() {
        for (int i = 0; i < inputSlot.Count; i++) {
            inputSlot[i].ClearSlot(true);
        }
        for (int i = 0; i < outputSlot.Count; i++) {
            outputSlot[i].ClearSlot(true);
        }
    }
}
