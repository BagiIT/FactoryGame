using System;
using System.Collections.Generic;
using SplineMeshTools.Core;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using SplineMesh = SplineMeshTools.Core.SplineMesh;

public class Builder : MonoBehaviour
{
    public static Builder Instance { get; private set; }
    public UnityAction<BuildingData,Vector3,Quaternion> OnBuildingBuilt;
    public UnityAction<BeltToBuild> OnBeltBuilt;
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
    private bool built = false;
    private bool hasMesh = false;

    [SerializeField]private Port firstPort = null;
    [SerializeField]private Port secondPort = null;
    [SerializeField]private Port lookingAtPort = null;
    [SerializeField]private GameObject previewBeltGO;
    [SerializeField]private SplineContainer previewSpline;
    [SerializeField] private SplineMesh previewMesh;
    [SerializeField] private float RotationAmount;

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

            if (previewBeltGO || buildingTempVisual) {
                if (PlayerInput.Instance.Input.Player.MouseWheel.ReadValue<float>() > 0.1f) AddRotation(1 * RotationAmount); 
                if (PlayerInput.Instance.Input.Player.MouseWheel.ReadValue<float>() < -0.1f) AddRotation(-1*RotationAmount);
            }

            if (PlayerManager.Instance.state == PlayerState.Building)
            {
                GetPlaceLocation();
            }
        }

        if (firstPort || secondPort) {
            if (firstPort) {
                Debug.DrawRay(firstPort.transform.position, firstPort.transform.forward, Color.green);
            }
            if (secondPort) {
                Debug.DrawRay(secondPort.transform.position, secondPort.transform.forward, Color.green);
            }
        }


    }

    private void AddRotation(float rotationAmount) {
        if (buildingTempVisual) {
            buildingTempVisual.transform.RotateAround(mainCamera.transform.position, Vector3.up, rotationAmount);
        }

        if (previewBeltGO) {
            previewBeltGO.transform.RotateAround(mainCamera.transform.position, Vector3.up, rotationAmount);
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
                if (Physics.Raycast(ray,out hit, PlaceRange, hitLayers)) {
                    if (!previewBeltGO) {
                        SpawnBeltHologram(hit.point);
                        hasMesh = true;
                    }if(!previewBeltGO.activeInHierarchy)
                    {
                        previewBeltGO.SetActive(true);
                    }

                    if (firstPort && secondPort) {
                        break;
                    }UpdateBelt(hit.point, this.gameObject.transform.rotation);
                }
                else {
                    if (previewBeltGO != null) {
                        previewBeltGO.SetActive(false);
                    }
                }
                if (Physics.Raycast(ray, out hit, PlaceRange, portRaycastLayers)) {
                    var port = hit.collider.GetComponent<Port>();
                    if (port == null) break;

                    if (port.connected) break;
                    lookingAtPort = port;
                    if (!previewBeltGO) {
                        SpawnBeltHologram(hit.point);
                        hasMesh = true;
                    }

                    if (firstPort && secondPort) {
                        break;
                    }
                    if (lookingAtPort) {
                        UpdateBelt(lookingAtPort.transform.position,lookingAtPort.transform.rotation);
                        break;
                    }
                   
                    break;
                }
                lookingAtPort = null;
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
    void SpawnBeltHologram(Vector3 position)
    {
        previewBeltGO = Instantiate(buildingSO.hologramPrefab, new Vector3(0,0,0), Quaternion.identity);
        previewSpline = previewBeltGO.GetComponent<SplineContainer>();
        previewMesh = previewBeltGO.GetComponent<SplineMesh>();
        previewSpline.Spline.Add(new BezierKnot(Vector3.zero));
        previewSpline.Spline.Add(new BezierKnot(Vector3.zero + new Vector3(0,0,1)));
        BeltBuilderUtils.GenereteBeltMesh(previewMesh);
    }

    void UpdateHologram(Vector3 position) {

        buildingTempVisual.transform.position = position;
        if (CanPlace() && HasItemsToBuild()) {
            CanBuild = true;
        }
        else {
            CanBuild = false;
        }

        //Debug.Log(CanBuild);
        if (CanBuild) {
            meshRender.material.color = Color.green;
        }
        else {
            meshRender.material.color = Color.red;
        }
    }


    public void UpdateBelt(Vector3 position, Quaternion rotation) {
        BeltBuilderUtils.UpdateBelt(position, rotation, previewBeltGO, previewSpline, previewMesh, firstPort,
            secondPort, lookingAtPort, this.transform);

        #region OldBeltBuilding Backup
/*
        if (!firstPort && !secondPort) {
            previewBeltGO.transform.position = position;
            previewBeltGO.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
        else if (!firstPort && secondPort && !lookingAtPort) {
            // Output is secondPort ➝ mouse is
            Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(secondPort.transform.position);
            Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(position);
            Vector3 tangentOut = GetTangentOutFromPort(secondPort.transform);

            Vector3 playerForward = this.transform.forward.normalized * 5f;

// Convert it to the local space of previewBeltGO
            Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(playerForward);


            BezierKnot knot0 = new BezierKnot {
                Position = localOutPos,
                TangentOut = (float3)tangentOut,
                TangentIn = float3.zero
            };

            BezierKnot knot1 = new BezierKnot {
                Position = localInPos,
                TangentIn = (float3)(-localTangentIn), // minus because it's "In"
                TangentOut = float3.zero
            };

            SetKnots(knot0, knot1);
        }else if (secondPort && (lookingAtPort != secondPort)) {
            // Output is secondPort ➝ mouse is
            Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(secondPort.transform.position);
            Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(lookingAtPort.transform.position);
            Vector3 tangentOut = GetTangentOutFromPort(secondPort.transform);


// Convert it to the local space of previewBeltGO
            Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(lookingAtPort.transform.forward.normalized *5f);


            BezierKnot knot0 = new BezierKnot {
                Position = localOutPos,
                TangentOut = (float3)tangentOut,
                TangentIn = float3.zero
            };

            BezierKnot knot1 = new BezierKnot {
                Position = localInPos,
                TangentIn = (float3)(localTangentIn), // minus because it's "In"
                TangentOut = float3.zero
            };

            SetKnots(knot0, knot1);
        }else if (firstPort && secondPort) {
            // Output is secondPort ➝ mouse is
            Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(secondPort.transform.position);
            Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(firstPort.transform.position);
            Vector3 tangentOut = GetTangentOutFromPort(secondPort.transform);


// Convert it to the local space of previewBeltGO
            Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(firstPort.transform.forward.normalized *5f);


            BezierKnot knot0 = new BezierKnot {
                Position = localOutPos,
                TangentOut = (float3)tangentOut,
                TangentIn = float3.zero
            };

            BezierKnot knot1 = new BezierKnot {
                Position = localInPos,
                TangentIn = (float3)(localTangentIn), // minus because it's "In"
                TangentOut = float3.zero
            };

            SetKnots(knot0, knot1);
        }


        GenereteBeltMesh();
    }
    Vector3 GetTangentOutFromPort(Transform port)
    {
        Vector3 worldTangentEnd = port.position + port.forward * 5f;
        return previewBeltGO.transform.InverseTransformPoint(worldTangentEnd) - previewBeltGO.transform.InverseTransformPoint(port.position);
    }

    Vector3 GetTangentInFromPort(Transform port)
    {
        Vector3 worldTangentEnd = port.position - port.forward * 5f;
        return previewBeltGO.transform.InverseTransformPoint(worldTangentEnd) - previewBeltGO.transform.InverseTransformPoint(port.position);
    }

    void SetKnots(BezierKnot knot0, BezierKnot knot1)
    {
        previewSpline.Spline.SetKnot(0, knot0);
        previewSpline.Spline.SetKnot(1, knot1);
        previewSpline.Spline.SetTangentMode(0, TangentMode.Broken, BezierTangent.Out);
        previewSpline.Spline.SetTangentMode(1, TangentMode.Broken, BezierTangent.In);
    }


    void GenereteBeltMesh() {
        previewMesh.GenerateMeshAlongSpline();
    }
*/
        #endregion
        
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
            previewMesh = null;
            previewSpline.Spline.Clear();
            previewSpline = null;
            
            Destroy(previewBeltGO);
            previewBeltGO = null;
            hasMesh = false;
        }
        
        built = false;
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
        OnBuildingBuilt?.Invoke(buildingSO,pos,buildingTempVisual.transform.rotation);
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

    private bool TrySelectPort(out Port port)
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray,out hit, PlaceRange, portRaycastLayers)) {
            if (hit.collider.TryGetComponent(out port)) {
                if (port.connected) {
                    port = null;
                    return false;
                }
                port.connected = true;
                Debug.DrawRay(hit.point, hit.collider.gameObject.transform.forward, Color.red);
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
            if (firstPort != null && secondPort != null) {
                ConfirmBuild();
            }
            else {
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
            }
            
            return true;
        }
        else
        {
            return TryPlaceBuilding(); // for regular buildings
            
        }
    }

    public bool CancleBuilding() {
        CleanUpHologram();
        if (firstPort != null) {
            firstPort.connected = false;
            firstPort = null;
        }
        if (secondPort != null) {
            secondPort.connected = false;
            secondPort = null;
        }

        if (lookingAtPort != null) {
            lookingAtPort = null;
        }
        return true;
    }

    public bool ConfirmBuild()
    {
        if (firstPort && secondPort && buildingSO.subCategory == SubCategory.Belt)
        {
            BeltToBuild belt = new BeltToBuild(buildingSO as BeltData, firstPort, secondPort);
            OnBeltBuilt?.Invoke(belt);
            built = true;
            CleanUpHologram();
            firstPort = null;
            secondPort = null;
            return true;
        }
        return false;
    }
}
