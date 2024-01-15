using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    public Transform DropPoint { get; }
    public KitchenObject KitchenObject { get; set; }
    public bool HasKitchenObject { get; }
    public void ClearKitchenObject();
    public NetworkObject GetNetworkObject();
}