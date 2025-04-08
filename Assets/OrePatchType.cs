using UnityEngine;
public enum OrePatchTypeEnum {
    Iron,
    Copper,
}
public class OrePatchType : MonoBehaviour {
    
    public OrePatchTypeEnum OreType;

    public OrePatchTypeEnum GetOreType() {
        return OreType;
    }
}
