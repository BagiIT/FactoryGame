using UnityEngine;

public interface IPort {
    bool CanPull();
    ItemData Pull();
    
    bool CanPush();
    bool Push(ItemData item);
}
