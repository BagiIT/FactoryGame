using System.Linq;
using UnityEngine;

public class BuildingRecepiesSelector : MonoBehaviour
{
    [SerializeField ]private GameObject slotPrefab;

    public void InitSlot(FactorySystem system)
    {
        ClearSlot();
        for (int i = 0; i < system.BuildingData.AvailableRecepies.Count; i++)
        {
           var slot = Instantiate(slotPrefab, transform);
            slot.GetComponent<BuildingRecepieButton>().Initi(system.BuildingData.AvailableRecepies[i], i);
        }
    }

    public void ClearSlot() {
        foreach (var item in gameObject.transform.Cast<Transform>())
        {
            Destroy(item.gameObject); //pooling better
        }
    }
}
