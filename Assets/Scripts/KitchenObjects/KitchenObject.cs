using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO _kitchenObjectSo;

    [SerializeField] private FollowTransform _followTransform;

    private IKitchenObjectParent _kitchenObjectParent;
    public KitchenObjectSO KitchenObjectSo => _kitchenObjectSo;
    public IKitchenObjectParent KitchenObjectParent
    {
        get => _kitchenObjectParent;
        set => _kitchenObjectParent = value;
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        _followTransform = GetComponent<FollowTransform>();
    }
#endif

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent, Vector3 offset = default)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject(), offset);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference koNetworkObjectReference, Vector3 offset)
    {
        SetKitchenObjectParentClientRpc(koNetworkObjectReference, offset);
    }
    
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference koNetworkObjectReference, Vector3 offset)
    {
        if (!koNetworkObjectReference.TryGet(out var koNetworkObject)) return;
        
        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent kitchenObjectParent))
        {
            _kitchenObjectParent?.ClearKitchenObject();
        
            _kitchenObjectParent = kitchenObjectParent;

            _kitchenObjectParent.KitchenObject = this;
            
            name = _kitchenObjectSo.objectName;

            SetTargetTransform(kitchenObjectParent.DropPoint, offset);
        }
    }

    public void SetTargetTransform(Transform targetTransform, Vector3 offset = default)
    {
        _followTransform.InitializeTarget(targetTransform, offset);
    }

    public void DestroySelf()
    {
       DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        DestroyClientRpc();
    }

    [ClientRpc]
    private void DestroyClientRpc()
    {
        _kitchenObjectParent?.ClearKitchenObject();
        
        Destroy(gameObject);
    }
}