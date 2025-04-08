using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots;


    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => InventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(BuildingData data)
    {

    }
    public InventorySystem(int size)
    {
        inventorySlots = new List<InventorySlot>(size);
        for(int i = 0; i < size; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddToInventory(ItemData itemToAdd, int amount)
    {
        if(ContainsItem(itemToAdd,out List<InventorySlot> inventorySlot))
        {
            foreach (var slot in inventorySlot)
            {
                if (slot.RoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
            
        }

        if (HasFreeSlot(out InventorySlot freeSlot))
        {
            freeSlot.UpdateInventorySlot(itemToAdd, amount);
            OnInventorySlotChanged?.Invoke(freeSlot);
            return true;
        }
            return false;
    }

    public bool RemoveFromInventory(ItemData itemToRemove, int amount)
    {
        if(ContainsItem(itemToRemove, out List<InventorySlot> inventorySlot))
        {
            foreach (var slot in inventorySlot)
            {
                if (amount <= 0)
                {
                    return true;
                }

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
            return amount <= 0;
        }
        return false;
    }

    public bool ContainsItem(ItemData itemToAdd, out List<InventorySlot> inventorySlot)
    {
        inventorySlot = InventorySlots.Where(i => i.Data == itemToAdd).ToList();

        return inventorySlot == null ? false : true;
    }

    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
        freeSlot = InventorySlots.FirstOrDefault(i => i.Data == null);
        return freeSlot == null ? false : true;
    }

}
