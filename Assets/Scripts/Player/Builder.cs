using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class Builder : MonoBehaviour
{
    public static Builder Instance { get; private set; }
    public UnityAction<BuildingData,Vector3> OnBuildingBuilt;
    public UnityAction<BuildingData, Port,Port> OnBeltBuilt;
    private Camera mainCamera;

    [SerializeField] private float PlaceRange;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private LayerMask BuildingCollisionLayers;
    [SerializeField] private LayerMask portRaycastLayers;
    [SerializeField] private float GridSize;
    [SerializeField] private float Offset;

    private BuildingData buildingSO;
    private GameObject buildingTempVisual;
    private RaycastHit hit;
    private Vector3 placePos;
    private bool CanBuild = false;

    [SerializeField]private Port firstPort = null;
    [SerializeField]private Port secondPort = null;
    [SerializeField]private GameObject previewBeltGO;
    [SerializeField]private SplineContainer previewSpline;

    private MeshRenderer meshRender;

    private RaycastHit[] hits;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayerManager.Instance.OnStateChanged += HandleStateChange;

    }
    private void OnDestroy()
    {

        PlayerManager.Instance.OnStateChanged -= HandleStateChange;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Production, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerManager.Instance.state == PlayerState.Building)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Production, 0);
                CleanUpHologram();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Production, 1);
                CleanUpHologram();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Production, 2);
                CleanUpHologram();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Production, 3);
                CleanUpHologram();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                buildingSO = BuildingStorage.Instance.GetBuilding(BuildingCategorys.Logistics, 0);
                CleanUpHologram();
            }

            if (PlayerManager.Instance.state == PlayerState.Building)
            {
                GetPlaceLocation();
            }
        }


    }
    void GetPlaceLocation()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        
        switch (buildingSO.subCategory) {
            case SubCategory.Miner:
                if (Physics.Raycast(ray, out hit, PlaceRange, hitLayers)) {
                    if (!buildingTempVisual)
                        SpawnHologramBuilding(hit.point);
                    if (hit.collider.gameObject.layer == 10) {
                        UpdateHologram(hit.collider.gameObject.transform.position);
                    }
                    else {
                        UpdateHologram(hit.point);
                    }
                }else
                {
                    Debug.DrawRay(ray.origin, ray.direction * PlaceRange, Color.red);
                    if (buildingTempVisual)
                        CleanUpHologram();
                }
                break;
            case SubCategory.Belt:
                hits = Physics.RaycastAll(ray, PlaceRange, portRaycastLayers);
                foreach (var hit in hits) {
                    var port = hit.collider.GetComponentInParent<Port>();
                    if(port == null) continue;
                    
                    Debug.DrawRay(hit.point, hit.normal * PlaceRange, Color.yellow);

                    if (firstPort == null) {
                        if(port.portType != Port.PortType.Output) continue;
                        
                    }
                    else {
                        if (port.portType != Port.PortType.Input || port == firstPort)continue;

                            PlaceBelt(firstPort, port);
                            //firstPort = null;
                            
                            if(previewBeltGO) Destroy(previewBeltGO);
                            previewBeltGO = null;

                    }
                    break;
                }

                
                break;
            default:
                if(Physics.Raycast(ray, out hit, PlaceRange, hitLayers))
                {
                    if (PlayerInput.Instance.Input.Player.SnapToGrid.IsPressed())
                    {
                        hit.point = SnapToGrid(hit.point);
                    }
                    Debug.DrawRay(ray.origin, ray.direction * PlaceRange, Color.green);

                    if (!buildingTempVisual)
                        SpawnHologramBuilding(hit.point);

                    UpdateHologram(hit.point);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * PlaceRange, Color.red);
                    if (buildingTempVisual)
                        CleanUpHologram();
                }
                break;
        }
        
    }

    void SpawnHologramBuilding(Vector3 position)
    {
        buildingTempVisual = Instantiate(buildingSO.hologramPrefab, position, Quaternion.identity);
        meshRender = buildingTempVisual.GetComponentInChildren<MeshRenderer>();
    }

    void UpdateHologram(Vector3 position)
    {
            buildingTempVisual.transform.position = position;
        if(CanPlace() && HasItemsToBuild())
        {
            CanBuild = true;
        }
        else
        {
            CanBuild = false;
        }
        //Debug.Log(CanBuild);
        if (CanBuild)
        {
            meshRender.material.color = Color.green;
        }
        else
        {
            meshRender.material.color = Color.red;
        }

    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        position -= Vector3.one * Offset;
        position /= GridSize;
        position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
        position *= GridSize;
        position += Vector3.one * Offset;
        return position;
    }

    void CleanUpHologram()
    {
        if (buildingTempVisual)
        {
            Destroy(buildingTempVisual);
            buildingTempVisual = null;
        }

        if (previewBeltGO) {
            Destroy(previewBeltGO);
            previewBeltGO = null;
            previewSpline = null;
            firstPort = null;
        }

        firstPort = null;
        secondPort = null;
    }

    public bool TryPlaceBuilding()
    {
        if (PlayerManager.Instance.state == PlayerState.Building)
        {
            if (CanBuild)
            {
                if (buildingSO.subCategory == SubCategory.Miner) {
                    if (hit.collider.gameObject.layer == 10) {
                        PlaceBuilding(hit.collider.gameObject.transform.position);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                PlaceBuilding(hit.point);
                return true;
            }
            else
                return false;
        }
        return false;
    }

    void PlaceBuilding(Vector3 pos)
    {
        OnBuildingBuilt?.Invoke(buildingSO,pos);
        UseItems();
        //processor.SetIndex(BuildingStorage.Instance.GetIndex(buildingSO));
    }

    private void HandleStateChange(PlayerState state)
    {
        if (state != PlayerState.Building)
            CleanUpHologram();
        else if (state == PlayerState.Building)
            CanBuild = HasItemsToBuild();
    }

    public void SetId(int id)
    {
        var tempdb =  Resources.Load<BuildingDatabase>("BuildingDatabase");
        buildingSO = tempdb.GetBuilding(id);
        Debug.Log(buildingSO.ID);
    }

    private bool HasItemsToBuild()
    {
        int tempcount = 0;
        int hasItems = 0;
        var playerInv = PlayerInventoryHolder.Instance.PrimaryInventorySystem;
        for (int i = 0; i < buildingSO.BuildingCost.Count; i++)
        {
            tempcount = 0;
            if (playerInv.ContainsItem(buildingSO.BuildingCost[i].item, out List<InventorySlot> slots))
            {
                //Debug.Log(buildingSO.BuildingCost[i].item.name);
                for (int j = 0; j < slots.Count; j++)
                {
                    tempcount += slots[j].StackSize;
                    //Debug.Log(tempcount);
                }
            }
            //Debug.Log(tempcount);
            if (tempcount >= buildingSO.BuildingCost[i].amount)
            {
                hasItems++;
            }
        }
        //Debug.Log(hasItems);
        //Debug.Log(buildingSO.BuildingCost.Count);
        if (hasItems == buildingSO.BuildingCost.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CanPlace()
    {
        var colliders = Physics.OverlapSphere(hit.point, 0.2f, BuildingCollisionLayers);

        switch (buildingSO.subCategory) {
            case SubCategory.Miner:
                Debug.Log($"{colliders.Length} miners + {hit.collider.gameObject.layer} layer");
                if (colliders.Length == 0 && hit.collider.gameObject.layer == 10) {
                    return true;
                }
                else {
                    return false;
                }
                break;
            default:
                for (int i = 0; i < colliders.Length; i++) {
                    //Debug.Log(colliders[i].name);
                    // Debug.Log(colliders[i].gameObject.layer);
                    if (colliders[i].gameObject.layer == 8)
                        return false;
                    break;
                }
                break;
        
        //Debug.Log(colliders.Length);
        
        }
        return true;
    }

    private void UseItems()
    {
        for (int i = 0; i < buildingSO.BuildingCost.Count; i++)
        {
            //Debug.Log(i);
            PlayerInventoryHolder.Instance.RemoveFromInventory(buildingSO.BuildingCost[i].item, buildingSO.BuildingCost[i].amount);
        }
    }

    /*private void StartPreviewSpline(Vector3 startPos) {
        previewBeltGO = Instantiate(buildingSO.hologramPrefab);
        previewSpline = previewBeltGO.GetComponent<SplineContainer>();
        
        previewSpline.Spline.Clear();
        
        previewSpline.Spline.Add(new BezierKnot(startPos));
        previewSpline.Spline.Add(new BezierKnot(startPos + Vector3.forward * 2f));
        previewSpline.Spline.Add(new BezierKnot(startPos + Vector3.forward * 4f));
        
    }

    void UpdatePreviewSpline(Vector3 startPos, Vector3 endPos) {
        if (previewSpline == null) return;
        
        Vector3 mid = Vector3.Lerp(startPos, endPos, 0.5f) + Vector3.up;
        
        previewSpline.Spline.SetKnot(0,new BezierKnot(startPos));
        previewSpline.Spline.SetKnot(1,new BezierKnot(mid));
        previewSpline.Spline.SetKnot(2,new BezierKnot(endPos));
    }*/

    private Vector3 GetMouseWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) {
            return hit.point;
        }
        return Vector3.zero;
    }

    void PlaceBelt(Port output, Port input) {
        firstPort = output;
        secondPort = input;
        
    }
    private bool TrySelectPort(out Port port)
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray,out hit, PlaceRange, portRaycastLayers)) {
            if (hit.collider.TryGetComponent(out port)) {
                return true;
            }
        }
        port = null;
        return false;
    }
    
    public bool HandleLeftClick()
    {
        if (buildingSO.subCategory == SubCategory.Belt)
        {
            if (TrySelectPort(out Port port))
            {
                if (firstPort == null)
                {
                    if (port.portType == Port.PortType.Output) {
                        secondPort = port;
                        Debug.Log("Selected Output port");
                    }
                    else {
                        firstPort = port;
                        Debug.Log("Selected input port");
                    }
                }else if (port != firstPort && port.portType != firstPort.portType) {
                    if (port.portType == Port.PortType.Input) {
                        firstPort = port;
                    }else if (port.portType == Port.PortType.Output) {
                        secondPort = port;
                    }
                }
            }
            return true;
        }
        else
        {
            return TryPlaceBuilding(); // for regular buildings
            
        }
    }

    public bool HandleConfirmBuild()
    {
        if (firstPort && secondPort && buildingSO.subCategory == SubCategory.Belt)
        {
            Vector3 midPoint = (firstPort.transform.position + secondPort.transform.position) / 2f;
            Debug.Log(firstPort.transform.position);
            Debug.Log(secondPort.transform.position);
            OnBeltBuilt?.Invoke(buildingSO,firstPort,secondPort);

            // Reset state
            firstPort = null;
            secondPort = null;
            return true;
        }
        return false;
    }
}
