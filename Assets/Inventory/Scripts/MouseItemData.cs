using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class MouseItemData : MonoBehaviour
{
    public Image ItemSprite;
    public TextMeshProUGUI ItemCount;
    public InventorySlot AssignedInventorySlot;

    private Transform playerTransform;

    

    private void Awake()
    {
        ItemSprite.color = Color.clear;
        ItemCount.text = "";
        ItemSprite.preserveAspect = true;
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        if(playerTransform == null)
        {
            Debug.Log("Player tag not found");
        }
    }
    public void UpdateMouseSlot(InventorySlot inventorySlot)
    {
        AssignedInventorySlot.AssignItem(inventorySlot);
        UpdateMouseSlot();
    }
    public void UpdateMouseSlot()
    {

        ItemSprite.sprite = AssignedInventorySlot.Data.uiIcon;
        ItemCount.text = AssignedInventorySlot.StackSize.ToString();
        ItemSprite.color = Color.white;
    }

    void Update()
    {
        if(AssignedInventorySlot.Data != null)
        {
            transform.position = Mouse.current.position.ReadValue();
            if(Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
            {
                Instantiate(AssignedInventorySlot.Data.ItemPrefab, playerTransform.position + playerTransform.forward * 3f,Quaternion.identity);

                if(AssignedInventorySlot.StackSize > 1)
                {
                    AssignedInventorySlot.AddToStack(-1);
                    UpdateMouseSlot();
                }
                else
                {
                    ClearSlot();
                }

            }

        }
    }

    public void ClearSlot()
    {
        AssignedInventorySlot.ClearSlot();
        ItemSprite.color = Color.clear;
        ItemCount.text = "";
        ItemSprite.sprite = null;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData evenDataCurrentPosition = new PointerEventData(EventSystem.current);
        evenDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(evenDataCurrentPosition, results);
        return results.Count > 0;
    }
}
