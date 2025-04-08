using System;
using UnityEngine;

public class Port : MonoBehaviour {
    public enum PortType {
        Input,Output
    }

    public PortType portType;
    public IPort portLogic;

    public FactorySystem connectedFactory;
}
