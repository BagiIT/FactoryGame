using System;
using UnityEngine;

public class Port : MonoBehaviour {
    public enum PortType {
        Input,Output
    }

    public PortType portType;
    public IPort portLogic;
    public bool connected = false;

    public FactorySystem connectedFactory;
}
