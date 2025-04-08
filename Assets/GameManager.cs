using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    
    private bool showTick = false;
    private void Awake() {
        Application.targetFrameRate = 144;
        frameDeltaTimeArray = new float[50];
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*void Start() {
        FactoryManager.OnTick += OnTick;
    }

    void OnDestroy() {
        FactoryManager.OnTick -= OnTick;
    }*/

    private void OnTick(int tick) {
        Debug.Log("Tick: "+tick);
    }

    // Update is called once per frame
    void Update()
    {
        #region fps

        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = Mathf.RoundToInt(CalculateFPS()).ToString();

        #endregion

        if (Keyboard.current.jKey.wasReleasedThisFrame) {
            ShowTick();
        }
    }

    private void ShowTick() {
        if (showTick) {
            showTick = false;
            FactoryManager.OnTick -= OnTick;
            return;
        }else if (!showTick) {
            showTick = true;
            FactoryManager.OnTick += OnTick;
            return;
        }
    }

    #region fpsFunction

    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    

    private float CalculateFPS() {
        float total = 0f;
        foreach (float deltaTime in frameDeltaTimeArray) {
            total += deltaTime;
        }
        return frameDeltaTimeArray.Length / total;
    }

    #endregion
}
