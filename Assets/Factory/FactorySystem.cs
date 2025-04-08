using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FactorySystem
{
    
    [SerializeField] private BuildingInventory buildingInventory;
    public UnityAction<int,int> OnProgressbarUpdate;

    public BuildingInventory BuildingInventory => buildingInventory;

    [SerializeField]private BuildingData data;

    [SerializeField]private RecipeData selectedRecipe= null;
    public RecipeData SelectedRecipe => selectedRecipe;
    public BuildingData BuildingData => data;

    [SerializeField] private int recepieId = -1;
    public int RecepieId => recepieId;

    private bool isProcessing = false;
    private int proessingTick = 0;
    private int tickToComplete = 0;


    #region  TestStuff
    [SerializeField]private bool InputCleared = false;
    [SerializeField]private bool OutputCleared = false;
    [SerializeField] private bool ResetRecipeDone = false;  
#endregion


[ShowInInspector] private List<IPort> inputPorts;

[ShowInInspector] private List<IPort> outPutPorts;
public IReadOnlyList<IPort> InputPorts => inputPorts;
public IReadOnlyList<IPort> OutputPorts => outPutPorts;
    public FactorySystem(BuildingData data)
    {
        buildingInventory = new BuildingInventory(data);
        Debug.Log(data.Name + data.BuildingPrefab);
        this.data = ScriptableObject.Instantiate(data);
        inputPorts = new List<IPort>();
        outPutPorts = new List<IPort>();
        inputPorts.Clear();
        outPutPorts.Clear();
        GeneratePorts();

        FactoryManager.OnTick10 += Tick10Update;
    }

    private void Tick10Update(int arg0) {
        TryPullInput();
        PushToOutput();
    }


    private void GeneratePorts() {
        if (data.inputPort > 0) {
            for (int i = 0; i < data.inputPort; i++) {
                inputPorts.Add(new ItemBuffer());
            }
        }

        if (data.outputPort > 0) {
            for (int i = 0; i < data.outputPort; i++) {
                outPutPorts.Add(new ItemBuffer());
            }
        }
    }

    public void CleanUp() {
        FactoryManager.OnTick -= TickUpdate;
    }



    private void TickUpdate(int tick) {
        if (selectedRecipe == null) return;

        Process();
    }

    public void SetReceipeID(int id)
    {
        recepieId = id;
        selectedRecipe = BuildingData.AvailableRecepies[recepieId];
        tickToComplete = Mathf.RoundToInt(selectedRecipe.processingTime);
        buildingInventory.SetSlotFilters(selectedRecipe);
        ResetRecipeDone = false;
        FactoryManager.OnTick += TickUpdate;
    }

    public void ClearRecipe() {
        CleanUp();
        if (selectedRecipe != null) {
            if (buildingInventory.InputSlot.Count >= 1) {
                for (int i = 0; i < buildingInventory.InputSlot.Count; i++) {
                    if(buildingInventory.InputSlot[i].Data == null) continue;
                    InputCleared = false;
                    if (PlayerInventoryHolder.Instance.PrimaryInventory.AddToInventory(buildingInventory.InputSlot[i].Data,
                            buildingInventory.InputSlot[i].StackSize)) {
                        InputCleared = true;
                    }
                }
            }
            

            for (int i = 0; i < buildingInventory.OutputSlot.Count; i++) {
                if(buildingInventory.OutputSlot[i].Data == null) continue;
                
                OutputCleared = false;
                if (PlayerInventoryHolder.Instance.PrimaryInventory.AddToInventory(buildingInventory.OutputSlot[i].Data,
                        buildingInventory.OutputSlot[i].StackSize)) {
                    OutputCleared = true;
                }
            }

            buildingInventory.ClearSlots();
        }
        selectedRecipe = null;
        recepieId = -1;
        isProcessing = false;
        proessingTick = 0;
        tickToComplete = 0;
        ResetRecipeDone = true;
    }

    public RecipeData GetRecipe() {
        return selectedRecipe;
    }

    public void Process() {
        if (isProcessing) {
            if (buildingInventory.HasEnoughItems(selectedRecipe)) {
                proessingTick += 1;
                OnProgressbarUpdate?.Invoke(proessingTick, tickToComplete);
                if (selectedRecipe != null) {
                    if (proessingTick >= selectedRecipe.processingTime) {
                        //processing CompleteDebug.Log("Has enoug items processing");
                        buildingInventory.AddToOutput(selectedRecipe.outputItem, selectedRecipe.outputAmount, 0);
                        for (int i = 0; i < selectedRecipe.inputItems.Count; i++) {
                            buildingInventory.RemoveFromInput(selectedRecipe.inputItems[i].item,
                                selectedRecipe.inputItems[i].amount, i);
                        }

                        /*for (int i = 0; i < selectedRecipe.inputItems.Count; i++) {
                            buildingInventory.RemoveFromInput(selectedRecipe.inputItems[i].item, selectedRecipe.inputItems[i].amount,i);
                        }*/
                        isProcessing = false;
                        proessingTick = 0;
                    }
                    else {
                        //still processing

                    }
                }

                return;
            }
            else {
                proessingTick = 0;
                OnProgressbarUpdate?.Invoke(proessingTick, tickToComplete);
                isProcessing = false;
            }
        }
        else {
            if (buildingInventory.HasEnoughItems(selectedRecipe)) {
                if (buildingInventory.HasOutpuSpace(selectedRecipe)) {
                    isProcessing = true;
                    for (int i = 0; i < selectedRecipe.inputItems.Count; i++) {
                        //remove from the UI
                    }
                    
                }
                else {
                    //Debug.Log("Not enoug space in output to process");
                }
            }
            else {
                Debug.Log("Hasnt enoug items processing" + data.Name + selectedRecipe.name);
            }
        }
    }

    void PushToOutput() {
        if (selectedRecipe != null) {
            for (int i = 0; i < OutputPorts.Count; i++) {
                var port = OutputPorts[i];
                if (buildingInventory.OutputSlot[i].Data != null && port.CanPush()) {
                    port.Push(buildingInventory.OutputSlot[i].Data);
                    buildingInventory.RemoveFromOutput(buildingInventory.OutputSlot[i].Data, 1, i);
                }
            }
        }
    }

    void TryPullInput() {
        if (selectedRecipe != null) {
            for (int i = 0; i < InputPorts.Count; i++) {
                var port = InputPorts[i];
                if (buildingInventory.InputSlot[i].Data == null) {
                    if (port.CanPull()) {
                        ItemData data = port.Pull();
                        if (data != null) {
                            if (buildingInventory.InputSlot[i].FilterItemData == data) {
                                buildingInventory.AddToInput(data, 1, i);
                            }
                            else {
                                for (int j = i; j < buildingInventory.InputSlot.Count; j++) {
                                    if (buildingInventory.InputSlot[j].FilterItemData == data) {
                                        buildingInventory.AddToInput(data, 1, j);
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    if (buildingInventory.InputSlot[i].StackSize < buildingInventory.InputSlot[i].Data.maxStack) {
                        if (port.CanPull()) {
                            ItemData data = port.Pull();
                            if (data != null) {
                                if (buildingInventory.InputSlot[i].FilterItemData == data) {
                                    buildingInventory.AddToInput(data, 1, i);
                                }
                                else {
                                    for (int j = i; j < buildingInventory.InputSlot.Count; j++) {
                                        if (buildingInventory.InputSlot[j].FilterItemData == data) {
                                            buildingInventory.AddToInput(data, 1, j);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
    }
}
