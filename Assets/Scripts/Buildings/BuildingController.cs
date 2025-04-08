using System;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


[System.Serializable]
[RequireComponent(typeof(UniqueID))]
public class BuildingController : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingSaveData buildingSaveData;
    [SerializeField] private List<Port> inputPorts;
    public List<Port> InputPorts => inputPorts;
    [SerializeField] private List<Port> outputPorts;
    public List<Port> OutputPorts => outputPorts;

    public static UnityAction<FactorySystem, int> OnBuildingInventoryRequest;
    public static UnityAction<FactorySystem> OnBuildingRecepieRequest;
    public static UnityAction OnBuildingCloseRecepieRequest;
    public static UnityAction<int,int> OnProgressbarUpdate;
    public static UnityAction<RecipeData> OnRecipeUpdate;

    private string id;
    private bool interacting = false;

    [SerializeField]private FactorySystem system;
    

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
        //BuildingRecepieButton.OnSelectedRecepie += system.SetReceipeID;
    }
    private void OnDisable()
    {
        //BuildingRecepieButton.OnSelectedRecepie -= system.SetReceipeID;
    }
    private void Start()
    {

        while (id == null)
            id = GetComponent<UniqueID>().ID;
        SaveGameManager.data.buildings.Add(id, buildingSaveData);
        
    }

    public void Init(FactorySystem system)
    {
        Debug.Log(system.BuildingData.ID);
        this.system = system;
        SetInputPorts();
        SetOutputPorts();
    }

    private void OnDestroy()
    {
        BuildingRecepieButton.OnRecepieSelected -= RecepieSelected;
        FactoryDynamicInventoeyDisplay.OnchangeRecipe -= OnChangeRecipe;
        system.OnProgressbarUpdate -= StartProgresBar;
        if (SaveGameManager.data.activeItems.ContainsKey(id)) SaveGameManager.data.activeItems.Remove(id);
    }

    public void Interact(Interactor interactor, out bool interactionSuccessful)
    {
        Debug.Log("Interacting..." + gameObject.name);

        if(system.RecepieId == -1)
        {
            //Doesnt have a set recepie
            //Show select recepies display
            OnBuildingRecepieRequest?.Invoke(system);
            PlayerManager.Instance.SetState(PlayerState.InMenu);
            Debug.Log("no recepie selected");
            BuildingRecepieButton.OnRecepieSelected += RecepieSelected;
            FactoryDynamicInventoeyDisplay.OnchangeRecipe += OnChangeRecipe;
            interactionSuccessful = true;
            return;
        }

        OnBuildingInventoryRequest?.Invoke(system, 0);
        PlayerInventoryHolder.Instance.OpenPlayerInventory();
        /*Debug.Log(system.Test1());
        system.Test();
        Debug.Log(system.Test2());*/
        ListInventory(system.BuildingInventory);
        FactoryDynamicInventoeyDisplay.OnchangeRecipe += OnChangeRecipe;
        system.OnProgressbarUpdate += StartProgresBar;
        interactionSuccessful = true;
    }

    


    public void EndInteraction()
    {
        BuildingRecepieButton.OnRecepieSelected -= RecepieSelected;
        FactoryDynamicInventoeyDisplay.OnchangeRecipe -= OnChangeRecipe;
        system.OnProgressbarUpdate -= StartProgresBar;
        interacting = false;
    }

    private void RecepieSelected(int id) {
        Debug.Log(id);
        system.SetReceipeID(id);
        OnBuildingCloseRecepieRequest?.Invoke();
        OnBuildingInventoryRequest?.Invoke(system,0);
        PlayerInventoryHolder.Instance.OpenPlayerInventory();
        OnRecipeUpdate?.Invoke(system.GetRecipe());
        system.OnProgressbarUpdate += StartProgresBar;
    }

    private void OnChangeRecipe() {
        Debug.Log("clearRecipe");
        system.OnProgressbarUpdate -= StartProgresBar;
        system.ClearRecipe();
        OnBuildingRecepieRequest?.Invoke(system);
    }

    public void ListInventory(BuildingInventory inv)
    {
        for (int i = 0; i < inv.InputSlot.Count; i++)
        {
            if(inv.InputSlot[i].Data != null)
            {
                Debug.Log("Inputs : " + inv.InputSlot[i].Data.name + inv.InputSlot[i].StackSize);
            }
            else
            {
                Debug.Log("Inputs : " + inv.InputSlot[i] + inv.InputSlot[i].StackSize);
            }
        }
        for (int i = 0; i < inv.OutputSlot.Count; i++)
        {
            if (inv.OutputSlot[i].Data != null)
            {
                Debug.Log("OutputSlot : " + inv.OutputSlot[i].Data.name + inv.OutputSlot[i].StackSize);
            }
            else
            {
                Debug.Log("OutputSlot : " + inv.OutputSlot[i] + inv.OutputSlot[i].StackSize);
            }
        }
    }
    
    private void StartProgresBar(int arg0,int arg1) {
        Debug.Log("BuildingCOntroller" + gameObject.name);
        OnProgressbarUpdate?.Invoke(arg0, arg1);
    }

    private void Update() {
        if (Keyboard.current.kKey.wasReleasedThisFrame) {
            system.Process();
        }
        if (Keyboard.current.tabKey.wasReleasedThisFrame) {
            EndInteraction();
        }
    }

    public void SetInputPorts() {
        for (int i = 0; i < system.InputPorts.Count; i++) {
            inputPorts[i].portLogic = system.InputPorts[i];
        }
    }
    public void SetOutputPorts() {
        for (int i = 0; i < system.OutputPorts.Count; i++) {
            outputPorts[i].portLogic = system.OutputPorts[i];
        }
    }
}
