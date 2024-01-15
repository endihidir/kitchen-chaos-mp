using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounter : NetworkBehaviour, IInteractable<BaseCounter>, IKitchenObjectParent
{
    #region VARIABLES

    [SerializeField] private KitchenObject _kitchenObject;
    public Transform DropPoint => _spawnPoint;
    public KitchenObject KitchenObject
    {
        get => _kitchenObject;
        set => _kitchenObject = value;
    }
    public bool HasKitchenObject => KitchenObject;
    
    [SerializeField] private Transform _spawnPoint;
    
    private event Action _onInteractionStart, _onInterrupt, _onComplete;

    #endregion

    #region PROPERTIES

    public virtual float ProgressDuration => 0f;

    #endregion

    public abstract void Interact(IKitchenObjectParent otherKitchenObjectParent);

    public BaseCounter OnStart(Action act)
    {
        _onInteractionStart = act;
        return this;
    }

    public BaseCounter OnInterrupt(Action act)
    {
        _onInterrupt = act;
        return this;
    }

    public BaseCounter OnComplete(Action act)
    {
        _onComplete = act;
        return this;
    }

    public void InvokeStart() => _onInteractionStart?.Invoke();
    public void InvokeInterrupt() => _onInterrupt?.Invoke();
    public void InvokeComplete() => _onComplete?.Invoke();
    
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }
    
    public NetworkObject GetNetworkObject() => NetworkObject;
}