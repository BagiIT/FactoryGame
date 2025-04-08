using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarDisplay : StaticInventoryDisplay
{
    private int _maxIndexSize = 9;
    private int _currentIndex = 0;

    private InputSystem_Actions inputAction;

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    protected override void Start()
    {
        base.Start();

        _currentIndex = 0;
        _maxIndexSize = slots.Length - 1;

        slots[_currentIndex].ToggleHighlight();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        inputAction.Enable();

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        inputAction.Disable();
    }

    private void Update()
    {
        if (inputAction.Player.MouseWheel.ReadValue<float>() > 0.1f) ChangeIndex(1); 
        if (inputAction.Player.MouseWheel.ReadValue<float>() < -0.1f) ChangeIndex(-1);
        if (inputAction.Player.UseItem.WasPressedThisFrame()) UseItem();
        if (inputAction.Player.DropItem.WasPerformedThisFrame()) DropItem();
    }

    private void UseItem()
    {
        if (slots[_currentIndex].AssignedInventorySlot.Data != null) slots[_currentIndex].AssignedInventorySlot.Data.UseItem();
    }
    private void DropItem()
    {
        if (slots[_currentIndex].AssignedInventorySlot.Data != null) slots[_currentIndex].DropItem();
    }

    private void ChangeIndex(int dir)
    {
        slots[_currentIndex].ToggleHighlight();
        _currentIndex += dir;

        if (_currentIndex > _maxIndexSize) _currentIndex = 0;
        if (_currentIndex < 0) _currentIndex = _maxIndexSize;

        slots[_currentIndex].ToggleHighlight();
    }

    private void SetIndex(int newIdex)
    {
        slots[_currentIndex].ToggleHighlight();
        if (newIdex < 0) _currentIndex = 0;
        if (newIdex > _maxIndexSize) newIdex = _maxIndexSize;

        _currentIndex = newIdex;
        slots[_currentIndex].ToggleHighlight();
    }
}
