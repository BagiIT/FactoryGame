using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

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

    private void BuildBuilding(BuildingData data, Vector3 pos)
    {
       
        var factorySystem = new FactorySystem(data);
        Factory.AddFactory(factorySystem);

        var building = Instantiate(data.BuildingPrefab, pos, Quaternion.identity);
        BuildingController bc = building.GetComponent<BuildingController>();
        bc.Init(factorySystem);
    }
    
    private void BuildBelt(BuildingData data, Port output, Port input) {
        var belt = new Belt(beltLenght: Vector3.Distance(output.transform.position, input.transform.position),
            itemsPerMinute: 60f,
            startPoint: output.transform.position,
            endPoint: input.transform.position,
            input: input.portLogic,
            outputPort: output.portLogic);
        Debug.Log("BELT CREATED");
        Factory.AddBelt(belt);
        Debug.Log("Place belt");
        Debug.Log(output.transform.position);
        Debug.Log(input.transform.position);
        Debug.Log(belt);
        PlaceBelt(data, output, input,belt);
        
    }

    private void PlaceBelt(BuildingData data,Port output, Port input,Belt belt) {
        GameObject finalBelt = Instantiate(data.BuildingPrefab);
        var visual = finalBelt.GetComponent<Belt_VisualController>();
        Debug.Log("Place belt");
        Debug.Log(output.transform.position);
        Debug.Log(input.transform.position);
        Debug.Log(belt);
        visual.logic = belt;
        visual.startPoint = input.transform;
        visual.endPoint = output.transform;
        var spline = finalBelt.GetComponent<SplineContainer>();
        spline.Spline.Clear();
        
        Vector3 start = output.transform.position;
        Vector3 end = input.transform.position;
        Vector3 mid = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.5f;
/*
        spline.Spline.Add(new BezierKnot(start));
        spline.Spline.Add(new BezierKnot(mid));
        spline.Spline.Add(new BezierKnot(end));*/
        var knotStart = new BezierKnot(output.transform.position);
        var knotMid = new BezierKnot(mid);
        var knotEnd = new BezierKnot(input.transform.position);

        knotStart.TangentOut = (mid - start) * 0.3f;
        knotEnd.TangentIn = (mid - end) * 0.3f;

        spline.Spline.Add(knotStart);
        spline.Spline.Add(knotMid);
        spline.Spline.Add(knotEnd);
        
        var splineInstantiate = finalBelt.GetComponent<SplineInstantiate>();
        if (splineInstantiate == null) { 
            splineInstantiate = finalBelt.AddComponent<SplineInstantiate>();
        }
        splineInstantiate.Container = spline;
        //splineInstantiate.UpdateInstances();
        
        //splineExtruder.Rebuild();

    }

    
    
}
