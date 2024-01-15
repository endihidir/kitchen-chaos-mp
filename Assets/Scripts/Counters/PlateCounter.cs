using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    #region VARIABLES

    [SerializeField] private KitchenObjectSO _plateKitchenObjectSO;

    private float _plateSpawnTime = 4f;

    #endregion

    #region PROPERTIES

    private CancellationTokenSource _plateSpawnCancellationTokenSource, _updateCancellationTokenSource;
    public KitchenObjectSO PlatePrefabKitchenObjectSO => _plateKitchenObjectSO;

    private event Action _onPlateSpawned;
    
    private event Action<IKitchenObjectParent> _onPlateTaken;

    #endregion

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _updateCancellationTokenSource = new CancellationTokenSource();
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        if (gamePlayState == GamePlayState.GamePlay)
        {
            SpawnPlateAsync();
        }
    }

    public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
    {
        TryTakePlateServerRpc(otherKitchenObjectParent.GetNetworkObject());
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TryTakePlateServerRpc(NetworkObjectReference koNetworkObjectReference)
    {
        if (!koNetworkObjectReference.TryGet(out var koNetworkObject)) return;
        
        if (koNetworkObject.TryGetComponent(out IKitchenObjectParent kitchenObjectParent))
        {
            if (HasKitchenObject && !kitchenObjectParent.HasKitchenObject)
            {
                SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);
 
                KillPlateSpawnToken();
        
                SpawnPlateAsync();
                
                _onPlateTaken?.Invoke(kitchenObjectParent);
            }
        }
    }

    private async void SpawnPlateAsync()
    {
        _plateSpawnCancellationTokenSource = new CancellationTokenSource();
        
        while (_updateCancellationTokenSource is not null)
        {
            if (IsServer)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_plateSpawnTime), DelayType.DeltaTime, PlayerLoopTiming.Update, _plateSpawnCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Plate counter task was cancelled!");
                    return;
                }
                
                SpawnPlateServerRpc();    
            }

            try
            {
                await UniTask.Yield(_updateCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Plate counter update cancelled!");
                return;
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        _onPlateSpawned?.Invoke();
    }

    public PlateCounter OnPlateSpawned(Action act)
    {
        _onPlateSpawned = act;
        return this;
    }

    public PlateCounter OnPlateTaken(Action<IKitchenObjectParent> act)
    {
        _onPlateTaken = act;
        return this;
    }

    private void KillPlateSpawnToken()
    {
        KillToken(ref _plateSpawnCancellationTokenSource);
    }

    private void KillUpdateToken()
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
        
        KillPlateSpawnToken();
        
        KillUpdateToken();
    }
}