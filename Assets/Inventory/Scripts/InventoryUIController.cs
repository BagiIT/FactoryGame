using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public DynamicInventoryDisplay invPanel;
    public DynamicInventoryDisplay playerBackPackPanel;
    public FactoryDynamicInventoeyDisplay factoryPanel;

    public BuildingRecepiesSelector RecepiesPanel;

    private void Awake()
    {
        invPanel.gameObject.SetActive(false);
        playerBackPackPanel.gameObject.SetActive(false);
        factoryPanel.gameObject.SetActive(false);
        RecepiesPanel.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested += DisplayPlayerInventory;
        BuildingController.OnBuildingInventoryRequest += DisplayBuildingInventory;
        BuildingController.OnBuildingRecepieRequest += DisplayBuildingRecepies;
        BuildingController.OnBuildingCloseRecepieRequest += CloseBuildingRecepies;
        BuildingController.OnProgressbarUpdate += OnProgresbarStart;
        BuildingController.OnRecipeUpdate += OnRecipeUpdate;
        //FactoryDynamicInventoeyDisplay.OnchangeRecipe += OnChangeRecipe;
    }

    


    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
        BuildingController.OnBuildingInventoryRequest -= DisplayBuildingInventory;
        BuildingController.OnBuildingRecepieRequest -= DisplayBuildingRecepies;
        BuildingController.OnBuildingCloseRecepieRequest -= CloseBuildingRecepies;
        BuildingController.OnProgressbarUpdate -= OnProgresbarStart;
        factoryPanel.UpdateProgressBar(0,0);
        BuildingController.OnRecipeUpdate -= OnRecipeUpdate;
        //FactoryDynamicInventoeyDisplay.OnchangeRecipe -= OnChangeRecipe;
    }

    


    private void DisplayInventory(InventorySystem invToDisplay,int offset)
    {
        invPanel.gameObject.SetActive(true);
        invPanel.RefreshDynamicInventory(invToDisplay, offset);
    }

    private void Update()
    {
        if (invPanel.gameObject.activeInHierarchy && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            invPanel.gameObject.SetActive(false);
            PlayerManager.Instance.CloseUI();
        }
        if (playerBackPackPanel.gameObject.activeInHierarchy && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            playerBackPackPanel.gameObject.SetActive(false);
            PlayerManager.Instance.CloseUI();
        }
        if (factoryPanel.gameObject.activeInHierarchy && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            factoryPanel.gameObject.SetActive(false);
            PlayerManager.Instance.CloseUI();
        }
        if (RecepiesPanel.gameObject.activeInHierarchy && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            CloseBuildingRecepies(true);
        }

    }
    private void DisplayPlayerInventory(InventorySystem invToDisplay, int offset)
    {
        playerBackPackPanel.gameObject.SetActive(true);
        playerBackPackPanel.RefreshDynamicInventory(invToDisplay, offset);
    }
    private void DisplayBuildingInventory(FactorySystem system, int offset)
    {
        factoryPanel.gameObject.SetActive(true);
        factoryPanel.RefreshDynamicInventory(system,system.BuildingInventory,offset);
    }
    private void DisplayBuildingRecepies(FactorySystem system)
    {
        if (!RecepiesPanel.gameObject.activeInHierarchy) {
            RecepiesPanel.gameObject.SetActive(true);
            RecepiesPanel.InitSlot(system);
        }

        if (factoryPanel.gameObject.activeInHierarchy) {
            factoryPanel.gameObject.SetActive(false);
        }
    }
    private void OnProgresbarStart(int arg0,int arg1) {
        factoryPanel.UpdateProgressBar(arg0,arg1);
    }

    private void CloseBuildingRecepies() {
        RecepiesPanel.gameObject.SetActive(false);
    }
    private void CloseBuildingRecepies(bool manualClose) {
        RecepiesPanel.gameObject.SetActive(false);
        PlayerManager.Instance.CloseUI();
    }
    private void OnRecipeUpdate(RecipeData arg0) {
        factoryPanel.UpdateInfo(arg0);
    }
    
    private void OnChangeRecipe() {
        factoryPanel.gameObject.SetActive(false);        
        playerBackPackPanel.gameObject.SetActive(false);
        RecepiesPanel.gameObject.SetActive(true);
    }
    
}
