using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Belt_VisualController : MonoBehaviour
{
    [ShowInInspector]
    public Belt logic;
    public GameObject itemPrefab;

    private List<GameObject> itemViews = new();

    public Transform startPoint;
    public Transform endPoint;

    void Start()
    {
        //logic = new Belt(120f, 5f,new Vector3(), new Vector3()); // 120 items/min, 5m long belt
    }

    void Update()
    {
        logic.Update(Time.deltaTime);

        SyncVisuals();
    }

    void SyncVisuals()
    {
        var items = logic.Items;
        
        // Add visuals if needed
        while (itemViews.Count < items.Count)
        {
            int index = itemViews.Count;
            var itemData = items[index].itemData;
            if (itemData == null) {
                itemViews.Add(null);
                continue;
            }

            GameObject visual = null;
            if (itemData.ItemPrefab != null) {
                visual = Instantiate(itemData.ItemPrefab,transform);
                visual.GetComponent<SphereCollider>().enabled = false;
            }
            else {
                // If just a Mesh is stored, create a quick mesh object
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube); // fallback
                visual.transform.SetParent(transform);
                visual.name = itemData.name;

                if (itemData.ItemPrefab != null) {
                    var mfi = itemData.ItemPrefab.GetComponent<MeshFilter>();
                    var mf = visual.GetComponent<MeshFilter>();
                    if (mf != null) mf.mesh = mfi.mesh;
                }

                if (itemData.ItemPrefab != null)
                {
                    var mri = itemData.ItemPrefab.GetComponent<MeshRenderer>();
                    var mr = visual.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = mri.material;
                }
            }
            itemViews.Add(visual);
        }

        // Remove excess visuals
        while (itemViews.Count > items.Count)
        {
            Destroy(itemViews[itemViews.Count - 1]);
            itemViews.RemoveAt(itemViews.Count - 1);
        }

        // Position items
        for (int i = 0; i < items.Count; i++)
        {
            float t = Mathf.Clamp01(items[i].position / logic.beltLenght);
            itemViews[i].transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
        }
    }

    public void PushItem(ItemData data)
    {
        logic.TryAddItem(data);
    }
}
