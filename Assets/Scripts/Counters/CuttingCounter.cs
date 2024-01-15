using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, ISwappable
{
    [SerializeField] private CuttingRecipeSO[] _cuttingRecipeSOArray;

    private string _unslicedObjectName;
    public override float ProgressDuration => GetRecipe(_unslicedObjectName)?.cutProgressTime ?? 0f;
    
    private CancellationTokenSource _sliceCancellationToken;

    private bool _isTaskStarted;
    public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
    {
        if (HasKitchenObject && otherKitchenObjectParent.HasKitchenObject)
        {
            var playerObjectName = otherKitchenObjectParent.KitchenObject.name;
            
            if (otherKitchenObjectParent.KitchenObject is PlateKitchenObject plateObject && plateObject.TryAddIngredient(this))
            {
                SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);
                InterruptKitchenObjectServerRpc();
                KitchenObject.DestroySelf();
            }
            else if (IsObjectSliceable(playerObjectName))
            {
                SwapIngredientsServerRpc(otherKitchenObjectParent.GetNetworkObject());
                SliceKitchenObjectServerRpc();
            }
        }
        else if(otherKitchenObjectParent.HasKitchenObject)
        {
            var playerObjectName = otherKitchenObjectParent.KitchenObject.name;
            
            if (IsObjectSliceable(playerObjectName))
            {
                SoundManager.Instance.PlaySound(SoundType.ObjectDrop, 0.35f);
                otherKitchenObjectParent.KitchenObject.SetKitchenObjectParent(this);
                SliceKitchenObjectServerRpc();
            }
        }
        else if (HasKitchenObject)
        {
            SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);
            KitchenObject.SetKitchenObjectParent(otherKitchenObjectParent);
            InterruptKitchenObjectServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwapIngredientsServerRpc(NetworkObjectReference otherKitchenObjectReference) => SwapIngredientsClientRpc(otherKitchenObjectReference);

    [ClientRpc]
    private void SwapIngredientsClientRpc(NetworkObjectReference otherKitchenObjectReference)
    {
        if (!otherKitchenObjectReference.TryGet(out var koNetworkObject)) return;
        
        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent playerKoObjectParentData))
        {
            SwapKitchenObjectParents(playerKoObjectParentData);
        }
    }

    public void SwapKitchenObjectParents(IKitchenObjectParent playerKoObjectParentData)
    {
        var playerObjectName = playerKoObjectParentData.KitchenObject.name;
        
        if(!IsObjectSliceable(playerObjectName) && !HasKitchenObject) return;
        
        if(!HasKitchenObject || !playerKoObjectParentData.HasKitchenObject) return;
        
        KitchenObject.SetTargetTransform(playerKoObjectParentData.DropPoint);
        playerKoObjectParentData.KitchenObject.SetTargetTransform(DropPoint);
      
        (KitchenObject.KitchenObjectParent, playerKoObjectParentData.KitchenObject.KitchenObjectParent) = 
            (playerKoObjectParentData.KitchenObject.KitchenObjectParent, KitchenObject.KitchenObjectParent);
        
        (KitchenObject, playerKoObjectParentData.KitchenObject) = (playerKoObjectParentData.KitchenObject, KitchenObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SliceKitchenObjectServerRpc() => SliceKitchenObjectAsync();
    
    [ServerRpc(RequireOwnership = false)]
    private void InterruptKitchenObjectServerRpc() => InterruptKitchenObjectClientRpc();

    [ClientRpc]
    private void InterruptKitchenObjectClientRpc()
    {
        KillSliceToken();
        InvokeInterrupt();
    }
    
    private async void SliceKitchenObjectAsync()
    {
        if (_isTaskStarted)
            KillSliceToken();

        _isTaskStarted = true;
        _unslicedObjectName = KitchenObject.name;
        
        InvokeStart();
        _sliceCancellationToken = new CancellationTokenSource();
        
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ProgressDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, _sliceCancellationToken.Token);
        }
        catch (OperationCanceledException e)
        {
            Debug.Log("Counter task was cancelled! : " + e);
            return;
        }

        SliceAction();
        _isTaskStarted = false;
    }
    
    private void SliceAction()
    {
        if(!HasKitchenObject) return;
        
        KitchenObject.DestroySelf();
        
        CreateSlices(_unslicedObjectName);
        
        InvokeComplete();
    }

    private void CreateSlices(string objectName)
    {
        var objectSO = GetSlicedKitchenObjectSo(objectName);
        KitchenObjectSpawner.Instance.SpawnKitchenObject(objectSO, this);
    }
    
    private void KillSliceToken()
    {
        _sliceCancellationToken?.Cancel();
        _sliceCancellationToken?.Dispose();
        _sliceCancellationToken = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        KillSliceToken();
    }

    private CuttingRecipeSO GetRecipe(string objectName) => _cuttingRecipeSOArray.FirstOrDefault(x => x.input.objectName.Equals(objectName));
    private KitchenObjectSO GetSlicedKitchenObjectSo(string objectName) => GetRecipe(objectName)?.output;
    private bool IsObjectSliceable(string objName) => _cuttingRecipeSOArray.Any(x=> x.input.objectName.Equals(objName));
}