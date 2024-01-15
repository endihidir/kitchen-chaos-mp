using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class KitchenObjectParentData
{ 
    [SerializeField] private KitchenObject _kitchenObject;

    private Transform _dropPoint;

    private NetworkObject _networkObject;
    public Transform DropPoint
    {
        get => _dropPoint;
        set => _dropPoint = value;
    }
    
    public KitchenObject KitchenObject
    {
        get => _kitchenObject;
        set => _kitchenObject = value;
    }
    public bool HasKitchenObject => KitchenObject;
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }
    
    public NetworkObject GetNetworkObject() => _networkObject;
    public void SetNetworkObject(NetworkObject networkObject) => _networkObject = networkObject;
}
