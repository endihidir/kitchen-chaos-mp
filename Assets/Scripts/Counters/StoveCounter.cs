using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, ISwappable
{
    [SerializeField] private FryingRecipeSO[] _fryingRecipeSoArray;
    
    private string _cookingObjectName;
    public override float ProgressDuration => GetRecipe(_cookingObjectName)?.fryProgressTime ?? 0f;

    private CancellationTokenSource _burntCancellationToken;
    
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
            else if (IsObjectHeatable(playerObjectName))
            {
                SwapIngredientsServerRpc(otherKitchenObjectParent.GetNetworkObject());
                CookKitchenObjectServerRpc();
            }
        }
        else if(otherKitchenObjectParent.HasKitchenObject)
        {
            var playerObjectName = otherKitchenObjectParent.KitchenObject.name;
            
            if (IsObjectHeatable(playerObjectName))
            {
                SoundManager.Instance.PlaySound(SoundType.ObjectDrop, 0.35f);
                otherKitchenObjectParent.KitchenObject.SetKitchenObjectParent(this);
                CookKitchenObjectServerRpc();
            }
        }
        else if (HasKitchenObject)
        {
            SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f); ;
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
        KitchenObject.SetTargetTransform(playerKoObjectParentData.DropPoint);
        playerKoObjectParentData.KitchenObject.SetTargetTransform(DropPoint);
      
        (KitchenObject.KitchenObjectParent, playerKoObjectParentData.KitchenObject.KitchenObjectParent) = 
            (playerKoObjectParentData.KitchenObject.KitchenObjectParent, KitchenObject.KitchenObjectParent);
        
        (KitchenObject, playerKoObjectParentData.KitchenObject) = (playerKoObjectParentData.KitchenObject, KitchenObject);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void CookKitchenObjectServerRpc() => CookKitchenObjectAsync();

    [ServerRpc(RequireOwnership = false)]
    private void InterruptKitchenObjectServerRpc() => InterruptKitchenObjectClientRpc();

    [ClientRpc]
    private void InterruptKitchenObjectClientRpc()
    {
        KillStoveToken();
        InvokeInterrupt();
    }

    private async void CookKitchenObjectAsync()
    {
        if (_isTaskStarted)
            KillStoveToken();

        _isTaskStarted = true;
        _cookingObjectName = KitchenObject.name;
        _burntCancellationToken = new CancellationTokenSource();

        while (HasKitchenObject && IsObjectHeatable(_cookingObjectName))
        {
            SoundManager.Instance.PlayLoopSoundAtPosition(SoundType.StoveSizzle, 0.1f, ProgressDuration);
            
            InvokeStart();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(ProgressDuration), DelayType.DeltaTime, PlayerLoopTiming.Update, _burntCancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Stove counter task was cancelled! : ");
                return;
            }

            CookAction();
        }

        _isTaskStarted = false;
        
        InvokeComplete();
    }

    private void CookAction()
    {
        if(!HasKitchenObject) return;
        KitchenObject.DestroySelf();
        CreateCooked(_cookingObjectName);
        _cookingObjectName = KitchenObject.name;
    }

    private void CreateCooked(string objectName)
    {
        var objectSO = GetFriedKitchenObjectSo(objectName);
        KitchenObjectSpawner.Instance.SpawnKitchenObject(objectSO, this);
    }

    private void KillStoveToken()
    {
        _isTaskStarted = false;
        SoundManager.Instance.RemoveLoopedSound(SoundType.StoveSizzle);
        _burntCancellationToken?.Cancel();
        _burntCancellationToken?.Dispose();
        _burntCancellationToken = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        KillStoveToken();
    }

    private FryingRecipeSO GetRecipe(string objectName) => _fryingRecipeSoArray.FirstOrDefault(x => x.input.objectName.Equals(objectName));
    private KitchenObjectSO GetFriedKitchenObjectSo(string objectName) => GetRecipe(objectName)?.output;
    private bool IsObjectHeatable(string objName) => _fryingRecipeSoArray.Any(x=> x.input.objectName.Equals(objName));
}