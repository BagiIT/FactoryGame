using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class TestingOUt : MonoBehaviour {
    private bool building = false;
    private bool firstPlace = false;

    Camera mainCamera;

    private RaycastHit hit;
    public LayerMask hitLayers;
    private GameObject beltObj;
    public Mesh box;
    private Spline spline;
    private GameObject meshObj;
    
    private SplineContainer splineContainer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    public float radius = 0.5f;
    public int sides = 4; // Box has 4 sides
    public int segments = 10;
    public bool capped = true;

    private void Start() {
        mainCamera = Camera.main;
    }

    void Update() {
        if (Keyboard.current.digit5Key.wasReleasedThisFrame) {
            building = !building;
            if (building) {
                TryStartSplineSegment(); // first preview segment
            } else {
                CleanUpHologram();
            }
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame && building) {
            PlaceBelt(hit.point);
        }

        if (building) {
            ShowHologram();
        }
    }

    void TryStartSplineSegment() {
        Vector3? startPoint = TrySnapToEndKnot();

        if (!startPoint.HasValue) {
            // Fallback to manual look direction raycast
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out hit, 200, hitLayers)) {
                startPoint = hit.point;
            }
        }

        if (startPoint.HasValue) {
            ShowBelt();
            PlaceBelt(startPoint.Value);
        }
    }

    Vector3? TrySnapToEndKnot() {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.SphereCast(ray, 0.5f, out hit, 200, hitLayers)) {
            var container = hit.collider.GetComponentInParent<SplineContainer>();
            if (container != null && container.Spline.Count > 0) {
                var endKnot = container.Spline[^1]; // Get the last knot in the spline
                return endKnot.Position;
            }
        }
        return null;
    }

    void ShowHologram() {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out hit, 200, hitLayers)) {
            Debug.DrawRay(ray.origin, ray.direction * 200, Color.green);

            if (firstPlace && spline.Count > 0) {
                // Live update the second knot preview while building
                var knot = new BezierKnot(hit.point);
                spline.SetKnot(1, knot);
                UpdateHologram();
            }

        } else {
            Debug.DrawRay(ray.origin, ray.direction * 200, Color.red);
            CleanUpHologram();
        }
    }
    void ShowBelt() {
        if (beltObj != null) {
            // If we're already in build mode and have an existing belt, clean up
            Destroy(beltObj);
        }

        beltObj = new GameObject("Belt");
        splineContainer = beltObj.AddComponent<SplineContainer>();
        spline = splineContainer.Spline;

        meshObj = new GameObject("SplineBoxMesh");
        meshObj.transform.SetParent(beltObj.transform);

        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshRenderer = meshObj.AddComponent<MeshRenderer>();

        building = true;

        // Immediately snap to the starting point of the first knot
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out hit, 200, hitLayers)) {
            PlaceBelt(hit.point); // Confirm the start point immediately when entering build mode
        }
    }

    private void CleanUpHologram() {
        if (beltObj) {
            Destroy(beltObj);
            beltObj = null;
            building = false;
            firstPlace = false; // Reset so it's ready to start fresh on next build
        }
    }
    
    void PlaceBelt(Vector3 position) {
        if (!firstPlace) {
            // Ensure the spline is initialized and has at least one knot
            if (spline.Count == 0) {
                var knot = new BezierKnot(position);
                spline.Add(knot); // Add the first knot
            }

            // Place the second knot temporarily
            var sknot = new BezierKnot(position);
            spline.Add(sknot); // Add the second knot (temporary until confirmed)
            firstPlace = true;

            // Immediately show the second knot location (preview the second position)
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out hit, 200, hitLayers)) {
                var previewKnot = new BezierKnot(hit.point);
                spline.SetKnot(1, previewKnot); // Temporarily set the second knot location
            }
        }
        else {
            // Confirm the second knot
            var knot = new BezierKnot(position);

            // Ensure spline has at least 2 knots before setting the second one
            if (spline.Count > 1) {
                spline.SetKnot(1, knot); // Finalize the second knot position
            }

            firstPlace = false;
            TryStartSplineSegment(); // Optionally start a new segment if needed
        }

        UpdateHologram(); // Update the mesh preview
    }

    void UpdateHologram() {

        Mesh mesh = new Mesh();
        SplineMesh.Extrude(
            splineContainer.Spline,
            mesh,
            radius,
            sides,
            segments,
            capped
        );

        meshFilter.mesh = mesh;

    }
}
