using System;
using UnityEngine;

public class CanavaReseter : MonoBehaviour
{
    private void Awake() {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(true);
        }
    }
}
