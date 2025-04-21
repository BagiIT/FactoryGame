using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class Belt_VisualController : MonoBehaviour
{
    [ShowInInspector]
    public Belt logic;
    public GameObject itemPrefab;

    private List<GameObject> itemViews = new();

    public Transform startPoint;
    public Transform endPoint;
    public SplineContainer spline;

    List<ItemInstance> itemInstances = new List<ItemInstance>();
    
    private Mesh fallbackMesh;
    private Material fallbackMaterial;

    void Awake()
    {
        fallbackMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        Shader fallbackShader = Shader.Find("Universal Render Pipeline/Lit");

        if (fallbackShader == null)
        {
            Debug.LogWarning("Standard shader not found! Falling back to Unlit/Color.");
            fallbackShader = Shader.Find("Unlit/Color"); // safe built-in shader
        }

        if (fallbackShader == null)
        {
            Debug.LogError("No fallback shader found! Belt visuals may not work.");
            // You could even disable visuals here if you want
        }

        fallbackMaterial = new Material(fallbackShader);
    }

    void Update()
    {
        logic.Update(Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.O)) {
            if (logic == null) {
                Debug.LogError("Belt_VisualController: logic is null");
            }
            logic.TryOutput();
        }
        SyncVisuals();
    }
/*
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
            itemViews[i].transform.position = spline.EvaluatePosition(t);
        }
    }
*/
    void SyncVisuals()
    {
        var items = logic.Items;
        itemInstances.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            var itemData = items[i].itemData;
            if (itemData == null) continue;

            Mesh mesh = null;
            Material[] materials = null;

            if (itemData.ItemPrefab != null)
            {
                var mfi = itemData.ItemPrefab.GetComponentInChildren<MeshFilter>();
                var mri = itemData.ItemPrefab.GetComponentInChildren<MeshRenderer>();

                if (mfi != null) mesh = mfi.sharedMesh;
                if (mri != null) materials = mri.sharedMaterials; // <<< IMPORTANT: sharedMaterials
            }

            if (mesh == null) mesh = fallbackMesh;
            if (materials == null || materials.Length == 0) materials = new Material[] { fallbackMaterial };
            
            

            itemInstances.Add(new ItemInstance
            {
                mesh = mesh,
                materials = materials,
                positionOnBelt = items[i].position
            });
        }
    }
    void LateUpdate()
    {
        foreach (var item in itemInstances)
        {
            float t = Mathf.Clamp01(item.positionOnBelt / logic.beltLenght);
            Vector3 pos = spline.EvaluatePosition(t);
            Quaternion rot = Quaternion.identity;
            Vector3 scale = Vector3.one;

            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scale);

            // Draw each submesh
            int submeshCount = Mathf.Min(item.mesh.subMeshCount, item.materials.Length);
            for (int submesh = 0; submesh < submeshCount; submesh++)
            {
                Graphics.DrawMesh(item.mesh, matrix, item.materials[submesh], 0, null, submesh);
            }
        }
    }
    
}
class ItemInstance
{
    public Mesh mesh;
    public Material[] materials;
    public float positionOnBelt;
}
