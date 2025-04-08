using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildingRecepieButton : MonoBehaviour
{
    [SerializeField] private TMP_Text recepieName;
    [SerializeField] private Image itemSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     private int id;

    public static UnityAction<int> OnRecepieSelected;
    public void Initi(RecipeData data,int id)
    {
        recepieName.text = data.name;
        itemSprite.sprite = data.outputItem.uiIcon;
        this.id = id;
    }

    public void SelectRecepie()
    {
        OnRecepieSelected?.Invoke(id);
    }


}
