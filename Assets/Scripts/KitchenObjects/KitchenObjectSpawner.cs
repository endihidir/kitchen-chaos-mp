using Unity.Netcode;
using UnityEngine;

public class KitchenObjectSpawner : NetworkBehaviour
{
    public static KitchenObjectSpawner Instance { get; private set; }

    [SerializeField] private KitchenObjectListSO _kitchenObjectListSo;

    private void Awake()
    {
        Instance = this;
    }
    
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSo, IKitchenObjectParent kitchenObjectParent, Vector3 offset = default)
    {
        var kitchenObjectSoIndex = GetKitchenObjectSoIndex(kitchenObjectSo);
        
        SpawnKitchenObjectServerRpc(kitchenObjectSoIndex, kitchenObjectParent.GetNetworkObject(), offset);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSoIndex, NetworkObjectReference koNetworkObjectReference, Vector3 offset)
    {
        var kitchenObjectSo = TryGetKitchenObjectSo(kitchenObjectSoIndex);
        
        if(!kitchenObjectSo) return;
        
        if (!koNetworkObjectReference.TryGet(out var koNetworkObject)) return;

        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent kitchenObjectParent))
        {
            if (kitchenObjectParent.HasKitchenObject && offset == default)
            {
                return;
            }
        }
      
        var kitchenObject = Instantiate(kitchenObjectSo.prefab);

        if (kitchenObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn(true);
        }
        
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent, offset);
    }

    private int GetKitchenObjectSoIndex(KitchenObjectSO kitchenObjectSo) => _kitchenObjectListSo.kitchenObjectList.IndexOf(kitchenObjectSo);

    private KitchenObjectSO TryGetKitchenObjectSo(int kitchenObjectSoIndex)
    {
        var kitchenObjectList = _kitchenObjectListSo.kitchenObjectList;
        
        if (kitchenObjectList.Count - 1 < kitchenObjectSoIndex || kitchenObjectList.Count < 1) return null;

        var kitchenObjectSo = kitchenObjectList[kitchenObjectSoIndex];

        return kitchenObjectSo;
    }
}
