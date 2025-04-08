using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;
    [SerializeField] private GameObject _slotHighlight;

    private Transform playerTransform;

    private Button button;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }
    private void Awake()
    {
        ClearSlot();

        itemSprite.preserveAspect = true;

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClick);

        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
        if(ParentDisplay == null)
        {
            ParentDisplay = transform.parent.parent.GetComponent<InventoryDisplay>();
        }
        if(ParentDisplay == null)
        {
            ParentDisplay = transform.parent.parent.parent.GetComponent<InventoryDisplay>();
        }
        //Debug.Log(ParentDisplay);
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }
    public void UpdateUISlot(InventorySlot slot)
    {
        if(slot.Data != null)
        {
            itemSprite.sprite = slot.Data.uiIcon;
            itemSprite.color = Color.white;

            if (slot.StackSize > 1) itemCount.text = slot.StackSize.ToString();
            else itemCount.text = "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void UpdateUISlot()
    {
        if (assignedInventorySlot != null) UpdateUISlot(assignedInventorySlot);
    }

    public void ClearSlot()
    {
        assignedInventorySlot?.ClearSlot();
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void OnUISlotClick()
    {
        ParentDisplay?.SlotClicked(this);
    }

    public void ToggleHighlight()
    {
        _slotHighlight.SetActive(!_slotHighlight.activeInHierarchy);
    }

    public void DropItem()
    {
        Instantiate(AssignedInventorySlot.Data.ItemPrefab, playerTransform.position + playerTransform.forward * 3f, Quaternion.identity);

        if (AssignedInventorySlot.StackSize > 1)
        {
            //Change to allow multiple item drops
            AssignedInventorySlot.RemoveFromStack(1);
            UpdateUISlot();
        }
        else
        {
            ClearSlot();
        }
    }

    public void SetRecipeBackground(Sprite sprite) {
        itemSprite.sprite = sprite;
        itemSprite.color = Color.black;
    }
}
