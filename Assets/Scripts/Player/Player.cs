using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    #region VARIABLES
    
    [SerializeField] private KitchenObject _kitchenObject;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private Animator _animator;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private Transform _kitchenObjectDropPoint;
    [SerializeField] private List<Vector3> _spawnPositionList;
    [SerializeField] private PlayerVisual _playerVisual;

    private Vector3 _lastMovementDir;

    private bool _isMovementSwitched;
    
    private IInteractable<BaseCounter> _lastInteractedObject;

    private float _footStepTimer, _footStepTime = 0.1f;

    private bool _isGamePlaying;
    
    #endregion

    #region PROPERTIES
    
    public Transform DropPoint => _kitchenObjectDropPoint;
    public KitchenObject KitchenObject
    {
        get => _kitchenObject;
        set => _kitchenObject = value;
    }
    
    public bool HasKitchenObject => KitchenObject;

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    #endregion

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Initialize();

        if (IsOwner)
        {
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        
            GameInput.Instance.OnInteractAction += OnInteractAction;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        
            GameInput.Instance.OnInteractAction -= OnInteractAction;

            GameInput.Instance.PlayerInputActions.Dispose();
        }
    }

    private void Initialize()
    {
        _animator = GetComponentInChildren<Animator>();
        _footStepTimer = _footStepTime;
        _isGamePlaying = GameManager.Instance.CurrentGameplayState == GamePlayState.GamePlay;

        var lobbyManager = PlayerConnectionManager.Instance;
        var playerData = lobbyManager.GetPlayerDataFromClientId(OwnerClientId);
        _playerVisual.SetPlayerColor(lobbyManager.GetPlayerColor(playerData.colorId));
        
        SetSpawnPos();
    }

    private void SetSpawnPos()
    {
        if(_spawnPositionList.Count < 1) return;

        var playerIndex = PlayerConnectionManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId);
        
        transform.position = _spawnPositionList[playerIndex];
    }
    
    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        _isGamePlaying = gamePlayState == GamePlayState.GamePlay;
    }

    private void OnInteractAction(object sender, EventArgs e)
    {
        if(!_isGamePlaying) return;
        
        _lastInteractedObject?.Interact(this);
    }
    
    private void Update()
    {
        if(!IsOwner) return;
        
        if (!_isGamePlaying)
        {
            SelectInteractable(null);
            ArrangeWalkAnimation(false);
            return;
        }
        
        var movementInput = GameInput.Instance.GetMovementVectorNormalized();

        HandleMovement(movementInput, Time.deltaTime);
        
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        var interactDistance = 1f;

        var isInteracted = Physics.Raycast(transform.position, _lastMovementDir, out var hit, interactDistance, _interactableLayer);

        if (!isInteracted)
        {
            SelectInteractable(null);
            return;
        }
        
        if (hit.transform.TryGetComponent(out IInteractable<BaseCounter> interactable))
        {
            if (interactable != _lastInteractedObject)
            {
                SelectInteractable(interactable);
            }
        }
        else
        {
            SelectInteractable(null);
        }
    }

    private void HandleMovement(Vector2 movementInput, float deltaTime)
    {
        var movementDir = new Vector3(movementInput.x, 0f, movementInput.y);

        if (movementDir != Vector3.zero)
            _lastMovementDir = movementDir;
        
        var moveDistance = _speed * deltaTime;
        var capsuleCastHeight = 2f;
        var capsuleRadius = 0.7f;

        var finalMovement = movementDir;

        var xMove = new Vector3(movementDir.x, 0f, 0f).normalized;
        var zMove = new Vector3(0f, 0f, movementDir.z).normalized;

        if (zMove.z is < -0.5f or > 0.5f && IsCollisionDetected(xMove, moveDistance, capsuleRadius, capsuleCastHeight))
        {
            finalMovement = zMove;
        }
        else if (xMove.x is < -0.5f or > 0.5f && IsCollisionDetected(zMove, moveDistance, capsuleRadius, capsuleCastHeight))
        {
            finalMovement = xMove;
        }

        if (!IsCollisionDetected(finalMovement, moveDistance, capsuleRadius, capsuleCastHeight))
        {
            transform.position += finalMovement * moveDistance;

            if (finalMovement.sqrMagnitude > 0.01f)
            {
                PlayFootStepSound(deltaTime);
            }
        }

        var isMovementActivated = movementDir.sqrMagnitude > 0.01f;

        RotatePlayer(isMovementActivated, movementDir, deltaTime);

        ArrangeWalkAnimation(isMovementActivated);
    }

    private void RotatePlayer(bool isMovementActivated, Vector3 movementDir, float deltaTime)
    {
        if(!isMovementActivated) return;

        transform.forward = Vector3.Slerp(transform.forward, -movementDir, 32f * deltaTime);
    }

    private void PlayFootStepSound(float deltaTime)
    {
        _footStepTimer -= deltaTime;

        if (!(_footStepTimer <= 0f)) return;
        
        _footStepTimer = _footStepTime;

        SoundManager.Instance.PlaySoundAtPosition(SoundType.FootStep, 1f, transform.position);
    }

    private void ArrangeWalkAnimation(bool isMovementActivated)
    {
        if (_isMovementSwitched == isMovementActivated) return;
        
        _isMovementSwitched = !_isMovementSwitched;

        _animator.SetBool(IsWalking, _isMovementSwitched);
    }
    
    private bool IsCollisionDetected(Vector3 dir, float distance, float radius, float height)
    {
        return Physics.BoxCast(transform.position, Vector3.one * radius, dir, Quaternion.identity,  distance, _obstacleLayer);
    }

    private void SelectInteractable(IInteractable<BaseCounter> interactable)
    {
        _lastInteractedObject = interactable;

        GameEvents.OnSelectedCounterChanged?.Invoke(_lastInteractedObject);
    }

    public void ClearKitchenObject() => KitchenObject = null;
    public NetworkObject GetNetworkObject() => NetworkObject;
}