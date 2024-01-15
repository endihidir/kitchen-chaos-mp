using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    
    private PlayerInputActions _playerInputActions;
    public PlayerInputActions PlayerInputActions => _playerInputActions;

    public event EventHandler OnInteractAction;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        _playerInputActions = new PlayerInputActions();
        
        _playerInputActions.Enable();

        _playerInputActions.Player.Interact.performed += OnInteractPerformed;

        _playerInputActions.Player.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();

        _playerInputActions.Player.Interact.performed -= OnInteractPerformed;

        _playerInputActions.Player.Pause.performed -= OnPausePerformed;
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = _playerInputActions.Player.Movement.ReadValue<Vector2>();

        return inputVector.normalized;
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnPausePerformed(InputAction.CallbackContext obj)
    {
        GameEvents.OnGamePause?.Invoke();
    }

}