
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    public Func<bool> HandleClickAction;
    public Func<bool> HandleRightClickAction;

    [SerializeField] private LayerMask buildingLayers;
    private Camera mainCamera;
    
    private RaycastHit hit;

    private InputSystem_Actions _input;
    public InputSystem_Actions Input => _input;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _input = new InputSystem_Actions();
        _input.Player.Disable();
        _input.UI.Disable();
    }
    private void Start()
    {
        mainCamera = Camera.main;
        PlayerManager.Instance.OnStateChanged += UpdateClickAction;
        UpdateClickAction(PlayerManager.Instance.state);
        _input.Player.Enable();
        PlayerManager.Instance.CloseUI();
    }

    private void OnDestroy()
    {
        PlayerManager.Instance.OnStateChanged -= UpdateClickAction;
    }

    void Update()
    {
        if (_input.Player.EnterBuildMode.WasPerformedThisFrame()) ToggleBuildMode();

        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            HandleClickAction?.Invoke();
        }

        if (UnityEngine.Input.GetMouseButtonDown(1)) {
            HandleRightClickAction?.Invoke();
        }
    }
    


    private void UpdateClickAction(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Building:
                HandleClickAction = () => Builder.Instance?.HandleLeftClick() ?? false;
                HandleRightClickAction = () => Builder.Instance?.HandleConfirmBuild() ?? false;
                break;
            case PlayerState.Default:
                HandleClickAction = DefaultClickAction;
                break;
        }
    }

    private bool DefaultClickAction()
    {
        Debug.Log("DefaultClick action");
        return false;
    }

    private void ToggleBuildMode()
    {
        if(PlayerManager.Instance.state == PlayerState.Default)
        {
            PlayerManager.Instance.SetState(PlayerState.Building);
        }
        else if(PlayerManager.Instance.state == PlayerState.Building)
        {
            PlayerManager.Instance.SetState(PlayerState.Default);
        }
    }

}
