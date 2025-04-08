using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.InputSystem;


public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;

    protected InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem;

    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    protected virtual void Start()
    {

    }

    public abstract void AssignSlot(InventorySystem invToDisplay,int offset);

    protected virtual void UpdateSlot(InventorySlot updatedSlot)
    {
        foreach (var slot in SlotDictionary)
        {
            if(slot.Value == updatedSlot) //Slot value - the " backend inv slot"
            {
                slot.Key.UpdateUISlot(updatedSlot); //Slot key - UI part
            }
        }
    }

    public void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        Debug.Log(clickedUISlot.AssignedInventorySlot.Filtered );
        if (clickedUISlot.AssignedInventorySlot.Filtered == 1 && mouseInventoryItem.AssignedInventorySlot.Data != null) {
            if (clickedUISlot.AssignedInventorySlot.FilterItemData != mouseInventoryItem.AssignedInventorySlot.Data) {
                Debug.Log("123123131 ");
                return;
            }
        }
        
        bool isSplitPressed = Keyboard.current.leftShiftKey.isPressed;
        //Debug.Log(clickedUISlot);
        //clicked slot item has mouse doesnt have item --pickup item

        //Debug.Log("pickup item");
        if (clickedUISlot.AssignedInventorySlot.Data != null && mouseInventoryItem.AssignedInventorySlot.Data == null)
        {
            //if shift hold split
            if (isSplitPressed && clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halftInventorySlot)) //split stack
            {
                mouseInventoryItem.UpdateMouseSlot(halftInventorySlot);
                clickedUISlot.UpdateUISlot();
                return;
            }
            else
            {
                mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                clickedUISlot.ClearSlot();
                return;
            }
        }

        //cliked slot doesnt have item mouse has item --place mouse into mepty slot
        //ebug.Log("place mouse into mepty slot");
        if (clickedUISlot.AssignedInventorySlot.Data == null && mouseInventoryItem.AssignedInventorySlot.Data != null)
        {
            //Debug.Log("place mouse into mepty slot");
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();
            mouseInventoryItem.ClearSlot();
            //Debug.Log("place mouse into mepty slot");
            return;
        }
       // Debug.Log("both have data chekc if same if so combine or swap");
        if (clickedUISlot.AssignedInventorySlot.Data != null && mouseInventoryItem.AssignedInventorySlot.Data != null)
        {
            bool same = clickedUISlot.AssignedInventorySlot.Data == mouseInventoryItem.AssignedInventorySlot.Data;
            if (same && clickedUISlot.AssignedInventorySlot.RoomLeftInStack(mouseInventoryItem.AssignedInventorySlot.StackSize))
            {
                clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
                clickedUISlot.UpdateUISlot();

                mouseInventoryItem.ClearSlot();
                return;
            }
            else if (same && !clickedUISlot.AssignedInventorySlot.RoomLeftInStack(mouseInventoryItem.AssignedInventorySlot.StackSize, out int leftInStack))
            {
                if (leftInStack < 1) SwapSlots(clickedUISlot); // stack full swap items
                else //slot not max fill from mouse
                {
                    int remainingOnMouse = mouseInventoryItem.AssignedInventorySlot.StackSize - leftInStack;
                    clickedUISlot.AssignedInventorySlot.AddToStack(leftInStack);
                    clickedUISlot.UpdateUISlot();

                    var newItem = new InventorySlot(mouseInventoryItem.AssignedInventorySlot.Data, remainingOnMouse);
                    mouseInventoryItem.ClearSlot();
                    mouseInventoryItem.UpdateMouseSlot(newItem);
                    return;
                }
            }
            else if (!same)
            {
                SwapSlots(clickedUISlot);
                return;
            }

        }



        //both have item -- items same combine-- is the slot stackSize + mouseStackSize > the slot max stac size > take from mouse  -- items diffrent swap



    }

    private void SwapSlots(InventorySlot_UI clickedUiSlot)
    {
        var tempSlot = new InventorySlot(mouseInventoryItem.AssignedInventorySlot.Data, mouseInventoryItem.AssignedInventorySlot.StackSize);
        mouseInventoryItem.ClearSlot();

        mouseInventoryItem.UpdateMouseSlot(clickedUiSlot.AssignedInventorySlot);

        clickedUiSlot.ClearSlot();
        clickedUiSlot.AssignedInventorySlot.AssignItem(tempSlot);
        clickedUiSlot.UpdateUISlot();
    }
}
