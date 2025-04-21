using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using SplineMesh = SplineMeshTools.Core.SplineMesh;

public class FactoryManager : MonoBehaviour {
    
    public static event UnityAction<int> OnTick;
    public static event UnityAction<int> OnTick10;
    
    public FactoryManager Instance { get; private set; }

    public BuildingDatabase buildingDatabase;

    public Factory Factory;
    //50 = 1s
    public const float FactoryTick_MAX = 0.02f; //should be close to 60 ticks per second add logic in the recipe for processing so i can input time in second and it convers it into tick time
    private float tickTimer = 0f;
    private int tick;

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
        tick = 0;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
    }
#if UNITY_EDITOR
    private void OnPlayModeChanged(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Cleanup();
        }
    }
#endif

    private void Cleanup() {
        Factory.ClearFactory();
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
    }
    
    private void Start()
    {
        Builder.Instance.OnBuildingBuilt += BuildBuilding;
        Builder.Instance.OnBeltBuilt += BuildBelt;
    }


    private void OnDestroy()
    {
        Builder.Instance.OnBuildingBuilt -= BuildBuilding;
        Builder.Instance.OnBeltBuilt -= BuildBelt;
        Cleanup();
    }

    private void Update() {
        tickTimer += Time.deltaTime;
        if (tickTimer >= FactoryTick_MAX) {
            tickTimer -= FactoryTick_MAX;
            tick++;
            if(tick % 10 == 0) {OnTick10?.Invoke(tick);}
            else {
                OnTick?.Invoke(tick);
            }
        }
    }

    private void BuildBuilding(BuildingData data, Vector3 pos,Quaternion rot)
    {
       
        var factorySystem = new FactorySystem(data);
        Factory.AddFactory(factorySystem);

        var building = Instantiate(data.BuildingPrefab, pos, rot);
        BuildingController bc = building.GetComponent<BuildingController>();
        bc.Init(factorySystem);
    }
    
    private void BuildBelt(BeltToBuild belt) {
        GameObject finalBelt = Instantiate(belt.BeltData.BuildingPrefab);
        var visual = finalBelt.GetComponent<Belt_VisualController>();
        var splineContainer = finalBelt.GetComponent<SplineContainer>();
        
        var splineMeshGen = finalBelt.GetComponent<SplineMesh>();
        BeltBuilderUtils.BuildBelt(finalBelt, splineContainer, splineMeshGen, belt.InputPort, belt.OutputPort);
        
        var beltsystem = new Belt(beltLenght: splineContainer.Spline.GetLength(),
            itemsPerMinute: belt.BeltData.ItemTransferSpeed ,
            startPoint: belt.OutputPort.transform.position,
            endPoint: belt.InputPort.transform.position,
            input: belt.OutputPort.portLogic,
            outputPort: belt.InputPort.portLogic);
        
        Factory.AddBelt(beltsystem);
        visual.logic = beltsystem;
        visual.startPoint = belt.OutputPort.transform;
        visual.endPoint = belt.InputPort.transform;
        visual.spline = splineContainer;


    }

    

    
    
}
