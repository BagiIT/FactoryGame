using UnityEngine;
using UnityEngine.Splines;
using SplineMesh = SplineMeshTools.Core.SplineMesh;


[CreateAssetMenu(fileName = "BeltData", menuName = "SO/BeltData", order = 1)]
public class BeltData : BuildingData
{
    public Mesh beltSegment;
    public int ItemTransferSpeed; //Per miniute
    
    public BeltData(BuildingData data,int itemTransferSpeed, Mesh beltSegment) : base(data) {
        ItemTransferSpeed = itemTransferSpeed;
        this.beltSegment = beltSegment;
    }
}

public class BeltToBuild {
    public BeltData BeltData;
    public Port InputPort;
    public Port OutputPort;

    public BeltToBuild(BeltData data, Port inputPort, Port outputPort) {
        BeltData = data;
        InputPort = inputPort;
        OutputPort = outputPort;
    }
}
