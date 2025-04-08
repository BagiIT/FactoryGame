using System;
using UnityEngine;
public enum PlayerState
{
    Default,
    Building,
    InMenu,
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public event Action<PlayerState> OnStateChanged;
    public event Action<GameObject> OnBuildingChanged;
    public PlayerState state
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            _state = value;
            OnStateChanged?.Invoke(_state);
        }
    }
    private PlayerState _state;

    private GameObject _buildingPrefab;
    public GameObject buildingPrefab
    {
        get => _buildingPrefab;
        private set
        {
            if (_buildingPrefab == value) return;
            _buildingPrefab = value;
            OnBuildingChanged?.Invoke(_buildingPrefab);
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        
    }
    public void SetState(PlayerState newState)
    {
        state = newState;
        switch (state)
        {
            case PlayerState.Default:
                Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;
                PlayerInput.Instance.Input.Player.Enable();
                PlayerInput.Instance.Input.UI.Disable();
                break;
            case PlayerState.Building:
                Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;
                PlayerInput.Instance.Input.Player.Enable();
                PlayerInput.Instance.Input.UI.Disable();
                Builder.Instance.SetId(0);
                break;
            case PlayerState.InMenu:
                Cursor.visible = true; Cursor.lockState = CursorLockMode.Confined;
                PlayerInput.Instance.Input.Player.Disable();
                PlayerInput.Instance.Input.UI.Enable();
                break;
        }
    }

    public void CloseUI()
    {
        SetState(PlayerState.Default);
    }
}
