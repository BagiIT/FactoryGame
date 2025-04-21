using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using SplineMesh = SplineMeshTools.Core.SplineMesh;

public static class BeltBuilderUtils
{
    public static void UpdateBelt(
        Vector3 position,
        Quaternion rotation,
        GameObject previewBeltGO,
        SplineContainer previewSpline,
        SplineMesh previewMesh,
        Port firstPort,
        Port secondPort,
        Port lookingAtPort,
        Transform playerTransform) {
        if (!firstPort && !secondPort) {
            HandleNoPorts(previewBeltGO, position, rotation);
        }
        else if (!firstPort && secondPort && !lookingAtPort) {
            HandleMouseToPort(previewBeltGO, previewSpline, secondPort.transform, position, playerTransform);
        }
        else if (secondPort && (lookingAtPort != secondPort)) {
            HandlePortToLookingAt(previewBeltGO, previewSpline, secondPort.transform, lookingAtPort.transform);
        }
        else if (firstPort && !secondPort && !lookingAtPort) {
            HandleMouseToPort(previewBeltGO, previewSpline, firstPort.transform, position, playerTransform);
        }
        else if (firstPort && (lookingAtPort != firstPort)) {
            HandlePortToLookingAt(previewBeltGO, previewSpline, firstPort.transform, lookingAtPort.transform);
        }
        else if (firstPort && secondPort) {
            HandlePortToPort(previewBeltGO, previewSpline, secondPort.transform, firstPort.transform);
        }

        GenereteBeltMesh(previewMesh);
    }

    public static void BuildBelt(
        GameObject previewBeltGO,
        SplineContainer previewSpline,
        SplineMesh previewMesh,
        Port firstPort,
        Port secondPort) {
        if (firstPort && secondPort) {
            SetupSpline(previewSpline);
            HandlePortToPort(previewBeltGO, previewSpline, secondPort.transform, firstPort.transform);
        }
        GenereteBeltMesh(previewMesh);
    }


    // === HANDLERS ===

    
    
    private static void HandleNoPorts(
        GameObject previewBeltGO,
        Vector3 position,
        Quaternion rotation )
    {
        previewBeltGO.transform.position = position;
        previewBeltGO.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
    }

    private static void HandleMouseToPort(
        GameObject previewBeltGO,
        SplineContainer previewSpline,
        Transform port,
        Vector3 mousePosition,
        Transform playerTransform)
    {
        Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(port.position);
        Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(mousePosition);
        Vector3 tangentOut = GetTangentOutFromPort(previewBeltGO, port);
        Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(playerTransform.forward.normalized * 5f);

        SetKnotPair(previewSpline, localOutPos, tangentOut, localInPos, -localTangentIn);
            
       
    }

    private static void HandlePortToLookingAt(
        GameObject previewBeltGO,
        SplineContainer previewSpline,
        Transform port,
        Transform lookingAtPort)
    {
        Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(port.position);
        Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(lookingAtPort.position);
        Vector3 tangentOut = GetTangentOutFromPort(previewBeltGO, port);
        Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(lookingAtPort.forward.normalized * 5f);

        SetKnotPair(previewSpline, localOutPos, tangentOut, localInPos, localTangentIn);
    }

    private static void HandlePortToPort(
        GameObject previewBeltGO,
        SplineContainer previewSpline,
        Transform port2,
        Transform port1)
    {
        Vector3 localOutPos = previewBeltGO.transform.InverseTransformPoint(port2.position);
        Vector3 localInPos = previewBeltGO.transform.InverseTransformPoint(port1.position);
        Vector3 tangentOut = GetTangentOutFromPort(previewBeltGO, port2);
        Vector3 localTangentIn = previewBeltGO.transform.InverseTransformDirection(port1.forward.normalized * 5f);

        SetKnotPair(previewSpline, localOutPos, tangentOut, localInPos, localTangentIn);
    }
    

    // === UTILS ===

    private static void SetKnotPair(
        SplineContainer splineContainer,
        Vector3 localOutPos,
        Vector3 tangentOut,
        Vector3 localInPos,
        Vector3 tangentIn)
    {
        BezierKnot knot0 = new BezierKnot
        {
            Position = localOutPos,
            TangentOut = (float3)tangentOut,
            TangentIn = float3.zero
        };

        BezierKnot knot1 = new BezierKnot
        {
            Position = localInPos,
            TangentIn = (float3)tangentIn,
            TangentOut = float3.zero
        };

        SetKnots(splineContainer, knot0, knot1);
    }

    public static Vector3 GetTangentOutFromPort(GameObject previewBeltGO, Transform port)
    {
        Vector3 worldTangentEnd = port.position + port.forward * 5f;
        return previewBeltGO.transform.InverseTransformPoint(worldTangentEnd) - previewBeltGO.transform.InverseTransformPoint(port.position);
    }

    public static Vector3 GetTangentInFromPort(GameObject previewBeltGO, Transform port)
    {
        Vector3 worldTangentEnd = port.position - port.forward * 5f;
        return previewBeltGO.transform.InverseTransformPoint(worldTangentEnd) - previewBeltGO.transform.InverseTransformPoint(port.position);
    }

    private static void SetKnots(SplineContainer splineContainer, BezierKnot knot0, BezierKnot knot1)
    {
        splineContainer.Spline.SetKnot(0, knot0);
        splineContainer.Spline.SetKnot(1, knot1);
        splineContainer.Spline.SetTangentMode(0, TangentMode.Broken, BezierTangent.Out);
        splineContainer.Spline.SetTangentMode(1, TangentMode.Broken, BezierTangent.In);
    }
    
    public static void GenereteBeltMesh(SplineMesh previewBeltMesh) {
        previewBeltMesh.GenerateMeshAlongSpline();
    }

    private static void SetupSpline(SplineContainer splaineContainer) {
        splaineContainer.Spline.Add(new BezierKnot(Vector3.zero));
        splaineContainer.Spline.Add(new BezierKnot(Vector3.zero + new Vector3(0,0,1)));
    }

}