using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateCounterVisual : BaseCounterVisual
{
    [SerializeField] private PlateCounter _plateCounter;

    private Stack<KitchenObject> _plateObjects = new Stack<KitchenObject>();

    private IKitchenObjectParent _myKitchenObjectParent;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        
        _plateCounter = GetComponent<PlateCounter>();
    }
#endif

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _myKitchenObjectParent = GetComponent<IKitchenObjectParent>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _plateCounter.OnPlateSpawned(OnPlateSpawned).OnPlateTaken(RemovePlateFromStack);
    }

    private void OnPlateSpawned()
    {
        if(_plateObjects.Count > 3) return;
        
        var plateSO = _plateCounter.PlatePrefabKitchenObjectSO;

        KitchenObjectSpawner.Instance.SpawnKitchenObject(plateSO, _myKitchenObjectParent, Vector3.up * (_plateObjects.Count * 0.1f));

        AddPlateClientRpc();
    }

    [ClientRpc]
    private void AddPlateClientRpc()
    {
        _plateObjects.Push(_myKitchenObjectParent.KitchenObject);
    }

    private void RemovePlateFromStack(IKitchenObjectParent otherKitchenObjectParent)
    {
        RemovePlateClientRpc(otherKitchenObjectParent.GetNetworkObject());
    }
    
    [ClientRpc]
    private void RemovePlateClientRpc(NetworkObjectReference otherKitchenObjectReference)
    {
        if (!otherKitchenObjectReference.TryGet(out var koNetworkObject)) return;
        
        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent otherKitchenObjectParent))
        {
            var takenKitchenObject = _plateObjects.Pop();

            if (takenKitchenObject)
            {
                takenKitchenObject.SetKitchenObjectParent(otherKitchenObjectParent);
            }
        }

        if (_plateObjects.Count > 0)
        {
            var lastMyKitchenObject = _plateObjects.Peek();

            if (lastMyKitchenObject)
            {
                lastMyKitchenObject.SetKitchenObjectParent(_myKitchenObjectParent, Vector3.up * ((_plateObjects.Count - 1) * 0.1f));
            }
        }
    }
}