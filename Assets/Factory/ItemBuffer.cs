using System.Collections.Generic;

public class ItemBuffer : IPort {
    
    private Queue<ItemData> buffer = new();
    private int capacity = 1;

    public bool CanPull() => buffer.Count > 0;

    public ItemData Pull()
    {
        return buffer.Count > 0 ? buffer.Dequeue() : null;
    }

    public bool CanPush() => buffer.Count < capacity;

    public bool Push(ItemData item)
    {
        if (CanPush())
        {
            buffer.Enqueue(item);
            return true;
        }

        return false;
    }
}
