using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FactoryDynamicInventoeyDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;
    [SerializeField] private GameObject InputPanel;
    [SerializeField] private GameObject OutputPanel;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text recipeName;
    [SerializeField] private Button changeRecipeButton;

    private float targetFill = 0f; // The target fill amount
    private float fillSpeed = 5f; // Speed of interpolation
    private int lastTick = 0;
    private FactorySystem tempFactory;
    
    public static UnityAction OnchangeRecipe;
    protected override void Start()
    {
        base.Start();
        
    }

    public void ChangeRecipe() {
        OnchangeRecipe?.Invoke();
    }
    
    

    public override void AssignSlot(InventorySystem invToDisplay, int offset)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();
        if (invToDisplay is BuildingInventory buildingInventory)
        {


        if (buildingInventory == null) return;



        for (int i = 0; i < buildingInventory.InputSlot.Count; i++)
        {
            var uiSlot = Instantiate(slotPrefab, InputPanel.transform);
            slotDictionary.Add(uiSlot, buildingInventory.InputSlot[i]);
            uiSlot.Init(buildingInventory.InputSlot[i]);
            uiSlot.UpdateUISlot();
            uiSlot.SetRecipeBackground(tempFactory.SelectedRecipe.inputItems[i].item.uiIconOutline);
        }
        for (int i = 0; i < buildingInventory.OutputSlot.Count; i++)
        {
            var uiSlot = Instantiate(slotPrefab, OutputPanel.transform);
            slotDictionary.Add(uiSlot, buildingInventory.OutputSlot[i]);
            uiSlot.Init(buildingInventory.OutputSlot[i]);
            uiSlot.UpdateUISlot();
            uiSlot.SetRecipeBackground(tempFactory.SelectedRecipe.outputItem.uiIconOutline);
            }
        }
    }

    public void RefreshDynamicInventory(FactorySystem system,InventorySystem invToDisplay, int offset)
    {
        tempFactory = system;
        ClearSlots();
        inventorySystem = invToDisplay;
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged += UpdateSlot;
        AssignSlot(invToDisplay, offset);
    }

    private void ClearSlots()
    {
        foreach (var item in InputPanel.transform.Cast<Transform>())
        {
            Destroy(item.gameObject); //pooling better
        }
        foreach (var item in OutputPanel.transform.Cast<Transform>())
        {
            Destroy(item.gameObject); //pooling better
        }


        if (slotDictionary != null) slotDictionary.Clear();
    }

    public void UpdateInfo(RecipeData recipe) {
        recipeName.text = recipe.name;
    }
    
    private void OnDisable()
    {
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }

    public void UpdateProgressBar(int currentTick, int ticksToComplete) {
        Debug.Log(currentTick + " / " + ticksToComplete);
        // Detect if progress should be reset
        if (currentTick == 1 && lastTick > 1)
        {
            // A new cycle has started, reset the fill
            progressBar.fillAmount = 0f;
            targetFill = 0f;
        }

        // Update target fill normally
        targetFill = Mathf.Clamp((float)(currentTick - 1) / (ticksToComplete - 1), 0f, 1f);

        // Store the last tick
        lastTick = currentTick;

        if (currentTick == ticksToComplete)
            targetFill = 1f;
    }

    private void Update() {
        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, targetFill, Time.deltaTime * fillSpeed);
    }
}
