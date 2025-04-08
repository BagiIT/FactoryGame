using System.Collections.Generic;
using NUnit.Framework;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class Belt
{
    public class BeltItem {
        public ItemData itemData;
        public float position;

        public BeltItem(ItemData itemData, float position) {
            this.itemData = itemData;
            this.position = position;
        }
    }
    
    //port input
    public IPort InputPort;
    public IPort OutputPort;
    //port output

    public float beltLenght;
    public float itemsPerMinute;
    public float minItemSpacing = 1f;
    
    [ShowInInspector, ReadOnly]
    private Vector3 startPosition;
    [ShowInInspector, ReadOnly]
    private Vector3 endPosition;

    
    private List<BeltItem> items = new();
    [ShowInInspector,ReadOnly]
    public IReadOnlyList<BeltItem> Items => items;

    public Belt(float beltLenght,float itemsPerMinute,Vector3 startPoint, Vector3 endPoint,IPort input,IPort outputPort) {
        this.beltLenght = beltLenght;
        this.InputPort = input;
        this.OutputPort = outputPort;
        this.startPosition = startPoint;
        this.endPosition = endPoint;
        SetSpeed(itemsPerMinute);
    }

    private float beltSpeed = 1f;

    public void SetSpeed(float newspeed) {
        itemsPerMinute = newspeed;
        float itemsPerSecond = itemsPerMinute / 60f;
        beltSpeed = itemsPerSecond * minItemSpacing;
    }

    public void Update(float deltaTime) {
        MoveItems(deltaTime);
        TryOutput();
        TryInput();
    }

    private void MoveItems(float deltaTime) {
            
        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            float maxMove = beltSpeed * deltaTime;
            float limit = beltLenght;
            if (i != 0) {
                var next = items[i - 1];
                limit = next.position - minItemSpacing;
            }
            else {
                if (item.position >= beltLenght) {
                    continue;
                }
            }
            item.position = Mathf.Min(item.position + maxMove, limit);
        }
    }

    private void TryOutput() {
        if (items.Count == 0) return;
        
        var front = items[0];
        if (front.position >= beltLenght && OutputPort?.CanPush() == true) {
            bool pushed = OutputPort.Push(front.itemData);
            if (pushed) {
                items.RemoveAt(0);
            }
        }
    }

    private void TryInput() {
        if (InputPort?.CanPull() != true)
            return;

        // Is there enough room to insert a new item?
        if (items.Count == 0 || items[items.Count - 1].position >= minItemSpacing)
        {
            var data = InputPort.Pull();
            if (data != null)
                items.Add(new BeltItem(data, 0f));
        }
    }
    public bool IsFull => items.Count > 0 && items[items.Count - 1].position < minItemSpacing;
    
    public bool TryAddItem(ItemData data)
    {
        if (items.Count == 0 || items[items.Count - 1].position >= minItemSpacing)
        {
            items.Add(new BeltItem(data, 0f));
            return true;
        }

        return false; // Not enough space to insert
    }
}
