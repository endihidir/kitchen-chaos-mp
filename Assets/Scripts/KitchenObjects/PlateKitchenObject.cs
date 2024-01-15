using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    [SerializeField] private KitchenObjectSO[] _validKitchenObjectSOList;

    [SerializeField] private PlateCompleteVisual _plateCompleteVisual;

    private readonly Dictionary<int, string> _kitchenObjects = new Dictionary<int, string>();
    public Dictionary<int, string> KitchenObjects => _kitchenObjects;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        _plateCompleteVisual = GetComponentInChildren<PlateCompleteVisual>(true);
    }
#endif
    public bool TryAddIngredient(IKitchenObjectParent kitchenObjectParent)
    {
        var objectId = kitchenObjectParent.KitchenObject.KitchenObjectSo.id;
        var objectName = kitchenObjectParent.KitchenObject.KitchenObjectSo.objectName;

        var isKitchenObjectNotValid = !_validKitchenObjectSOList.Any(x => x.objectName.Equals(objectName));

        var isKitchenObjectAlreadyAdded = _kitchenObjects.ContainsKey(objectId);

        if (isKitchenObjectAlreadyAdded || isKitchenObjectNotValid) return false;
       
        AddIngredientsServerRpc(objectId, objectName, kitchenObjectParent.GetNetworkObject());
        
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientsServerRpc(int objectId, string objectName, NetworkObjectReference kitchenObjectParentReference)
    {
        AddIngredientsClientRpc(objectId, objectName, kitchenObjectParentReference);
    }
    
    [ClientRpc]
    private void AddIngredientsClientRpc(int objectId, string objectName, NetworkObjectReference kitchenObjectParentReference)
    {
        if (!kitchenObjectParentReference.TryGet(out var koNetworkObject)) return;
        
        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent kitchenObjectParent))
        {
            _plateCompleteVisual.CreateVisual(kitchenObjectParent.KitchenObject.KitchenObjectSo);
        
            _kitchenObjects.Add(objectId, objectName);
        }
    }
}