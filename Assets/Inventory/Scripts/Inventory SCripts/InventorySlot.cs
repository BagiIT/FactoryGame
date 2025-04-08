using NUnit.Framework.Internal.Execution;
using System;
using UnityEngine;

[System.Serializable]
public class InventorySlot : ISerializationCallbackReceiver
{
    [NonSerialized] private ItemData itemData;
    [SerializeField] private int itemId = -1;
    [SerializeField] private int stackSize;
    [SerializeField] private int filtered = -1;
    [SerializeField] private ItemData filterItemData;

    public ItemData Data => itemData;
    public int StackSize => stackSize;
    public int Filtered => filtered;
    public ItemData FilterItemData => filterItemData;

    public InventorySlot(ItemData source, int amount,int filter, ItemData filterItemData)
    {
        itemData = source;
        itemId = itemData.ID;
        stackSize = amount;
        filtered = filter;
        this.filterItemData = filterItemData;
    }
    
    public InventorySlot(ItemData source, int amount)
    {
        itemData = source;
        itemId = itemData.ID;
        stackSize = amount;
        filtered = -1;
        filterItemData = null;
    }
    
    public InventorySlot()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        itemData = null;
        itemId = -1;
        stackSize = -1;
    }

    public void ClearSlot(bool filtered) {
        itemData = null;
        itemId = -1;
        stackSize = -1;
        this.filtered = -1;
        filterItemData = null;
    }
    public void UpdateInventorySlot(ItemData data, int amount, int filter, ItemData filterItem)
    {
        itemData = data;
        itemId = itemData.ID;
        stackSize = amount;
        filtered = filter;
        filterItemData = filterItem;
    }

    public void UpdateInventorySlot(ItemData data, int amount)
    {
        itemData = data;
        itemId = itemData.ID;
        stackSize = amount;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountLeft)
    {
        amountLeft = itemData.maxStack - stackSize;
        return RoomLeftInStack(amountToAdd);
    }
    public bool RoomLeftInStack(int amountToAdd)
    {
        if (stackSize + amountToAdd <= itemData.maxStack) return true;
        else return false;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }

    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
    }

    public void AssignItem(InventorySlot invSlot)
    {
        if(itemData == invSlot.itemData)
        {
            AddToStack(invSlot.stackSize);
        }
        else
        {
            itemData = invSlot.itemData;
            itemId = itemData.ID;
            stackSize = 0;
            AddToStack(invSlot.stackSize);
        }
    }

    public bool SplitStack(out InventorySlot splitStack)
    {
        if(stackSize <= 1)
        {
            splitStack = null;
            return false;
        }
            int halfStack = Mathf.RoundToInt(stackSize / 2);
            RemoveFromStack(halfStack);

            splitStack = new InventorySlot(itemData, halfStack);
            return true;
    }

    public void AssignFilter(int filter, ItemData filterItem) {
        filtered = filter;
        filterItemData = filterItem;
    }

    #region Interface
    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        if (itemId == -1) return;

        var tempdb = Resources.Load<ItemDatabase>("ItemDatabase");
        itemData = tempdb.GetItem(itemId);
    }

    #endregion
}
