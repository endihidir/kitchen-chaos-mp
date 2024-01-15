using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : NetworkBehaviour
{ 
    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO _recipeListSo;

    private List<RecipeSO> _waitingRecipeSOList = new List<RecipeSO>();
    
    private float _maxSpawnRecipeTimer = 4f;

    private CancellationTokenSource _recipeCancellationToken, _updateCancellationTokenSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        _updateCancellationTokenSource = new CancellationTokenSource();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        if (gamePlayState == GamePlayState.GamePlay)
        {
            StartRecipe();
        }
        else if (gamePlayState == GamePlayState.GameOver)
        {
            DisposeRecipeToken();
        }
    }

    private async void StartRecipe()
    {
        _recipeCancellationToken = new CancellationTokenSource();
        
        while (_updateCancellationTokenSource is not null)
        {
            if (IsServer)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_maxSpawnRecipeTimer), DelayType.DeltaTime, PlayerLoopTiming.Update, _recipeCancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Delivery Task Cancelled! ");
                    return;
                }
            
                PrepareRecipe();
            }

            try
            {
                await UniTask.Yield(_updateCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Delivery manager update cancelled!");
                return;
            }
        }
    }

    private void PrepareRecipe()
    {
        if (_waitingRecipeSOList.Count < _recipeListSo.recipeSOList.Count)
        {
            var randomRecipeIndex = Random.Range(0, _recipeListSo.recipeSOList.Count);

            PrepareRecipeClientRpc(randomRecipeIndex);
        }
    }

    [ClientRpc]
    private void PrepareRecipeClientRpc(int index)
    {
        var recipeSO = _recipeListSo.recipeSOList[index];
        
        GameEvents.OnSpawnRecipe?.Invoke(recipeSO);

        _waitingRecipeSOList.Add(recipeSO);
    }

    public bool TryDeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        if (_waitingRecipeSOList.Count < 1)
        {
            DeliverInCorrectRecipeServerRpc();
            return false;
        }

        for (int i = 0; i < _waitingRecipeSOList.Count; i++)
        {
            var waitingRecipeSO = _waitingRecipeSOList[i];
            
            var isRecipeCountsSame = waitingRecipeSO.kitchenObjectSoArray.Length.Equals(plateKitchenObject.KitchenObjects.Count);
             
            if (isRecipeCountsSame)
            {
                var isAllRecipeSame = waitingRecipeSO.kitchenObjectSoArray.All(x => plateKitchenObject.KitchenObjects.ContainsValue(x.objectName));

                if (isAllRecipeSame)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return true;
                }
            }
            
        }
        
        DeliverInCorrectRecipeServerRpc();
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        DisposeRecipeToken();
        StartRecipe();
        
        SoundManager.Instance.PlaySound(SoundType.DeliverySuccess, 0.35f);
        
        var waitingRecipeSO = _waitingRecipeSOList[waitingRecipeSOListIndex];
        GameEvents.OnDeliverRecipe?.Invoke(waitingRecipeSO);
        
        _waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeliverInCorrectRecipeServerRpc()
    {
        DeliverInCorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverInCorrectRecipeClientRpc()
    {
        SoundManager.Instance.PlaySound(SoundType.DeliveryFail, 0.05f);
    }
    
    private void DisposeRecipeToken()
    {
        KillToken(ref _recipeCancellationToken);
    }
    
    private void DisposeUpdateToken()
    {
        KillToken(ref _updateCancellationTokenSource);
    }
    
    private void KillToken(ref CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        DisposeRecipeToken();
        DisposeUpdateToken();
    }
}