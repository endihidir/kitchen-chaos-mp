using System;
using Unity.Netcode;

public interface IInteractable<T>
{ 
    public void Interact(IKitchenObjectParent otherKitchenObjectParent);
    public float ProgressDuration { get;}
    public T OnStart(Action act);
    public T OnInterrupt(Action act);
    public T OnComplete(Action act);
    public void InvokeStart();
    public void InvokeInterrupt();
    public void InvokeComplete();
    public NetworkObject GetNetworkObject();
}