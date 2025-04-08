using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class PlayerInventoryHolder : InventoryHolder
{
    public static PlayerInventoryHolder Instance { get; private set; }
    public InventorySystem PrimaryInventory => primaryInventorySystem;

    public static UnityAction OnPlayerInventoryChanged;
    public static UnityAction<InventorySystem, int> OnPlayerInventoryDisplayRequested;
    

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        SaveGameManager.data.playerInventory = new InventorySaveData(primaryInventorySystem);
    }
    

    protected override void LoadInventory(SaveData arg0)
    {
        //check save data for this chest inv if exits loads it
        Debug.Log(arg0);
        if (arg0.playerInventory.inventorySystem != null)
        {
            this.primaryInventorySystem = arg0.playerInventory.inventorySystem;
            OnPlayerInventoryChanged?.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame) OpenPlayerInventory();
    }

    public bool AddToInventory(ItemData data, int amount)
    {
        if (primaryInventorySystem.AddToInventory(data, amount))
        {
            return true;
        }
        
        return false;
    }
    public bool RemoveFromInventory(ItemData data, int amount)
    {
        if (primaryInventorySystem.RemoveFromInventory(data, amount))
        {
            return true;
        }
        return false;
    }

    public void OpenPlayerInventory()
    {
        OnPlayerInventoryDisplayRequested?.Invoke(primaryInventorySystem, Offset);
        PlayerManager.Instance.SetState(PlayerState.InMenu);

    }

}
