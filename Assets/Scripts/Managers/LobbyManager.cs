using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private Lobby _joinedLobby;
    public Lobby Lobby => _joinedLobby;

    private float _heartBeatTimer, _listLobbiesTimer;
    private bool IsLobbyHost => _joinedLobby is not null && _joinedLobby.HostId.Equals(AuthenticationService.Instance.PlayerId);

    private CancellationTokenSource _updateCancellationToken;
    
    public static bool playMultiplayer;

    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeUnityAuthenticationAsync();
    }

    private void Start()
    {
        _updateCancellationToken = new CancellationTokenSource();
        
        GameEvents.OnEnableLobbyFadePanel?.Invoke(!playMultiplayer);
        
        HandleHeartbeatAsync();
        
        RefreshLobbyAsync();
    }

    private async void InitializeUnityAuthenticationAsync()
    {
        if (UnityServices.State.Equals(ServicesInitializationState.Uninitialized))
        {
            var initializationOptions = new InitializationOptions();
            
            initializationOptions.SetProfile(Random.Range(0, 10000).ToString());
            
            await UnityServices.InitializeAsync(initializationOptions);
            
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (!playMultiplayer)
            {
                await RelayManager.Instance.CreateRelayAsync();

                PlayerConnectionManager.Instance.StartHost();

                await Loader.Load(Scene.GameScene, true, false);
            }
        }
    }

    private async void HandleHeartbeatAsync()
    {
        while (_updateCancellationToken is not null)
        {
            if (IsLobbyHost)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }

            var isOperationCancelled = await WaitAsync(Constants.HEART_BEAT_TIMER_MAX);

            if (isOperationCancelled)
            {
                return;
            }
        }
    }

    private async void RefreshLobbyAsync()
    {
        while (_updateCancellationToken is not null)
        {
            if (_joinedLobby is null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name.Equals(Scene.LobbyScene.ToString()))
            {
                await ListLobbiesAsync();
            }

            var isOperationCancelled = await WaitAsync(Constants.LIST_LOBBIES_TIMER_MAX);
            
            if (isOperationCancelled)
            {
                return;
            }
        }
    }

    private async UniTask<bool> WaitAsync(float delay)
    {
        try
        {
            var dly = (int)(delay * 1000);

            await UniTask.Delay(dly, DelayType.DeltaTime, PlayerLoopTiming.Update, _updateCancellationToken.Token);
        }
        catch (OperationCanceledException e)
        {
            Debug.Log(e);
            return true;
        }

        return false;
    }

    private async UniTask ListLobbiesAsync()
    {
        try
        {
            var queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Filters = new List<QueryFilter>()
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
    
            GameEvents.OnLobbyListChanged?.Invoke(queryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Listing Lobby failed!" + e);
        }
    }

    public async UniTask CreateLobbyAsync(string lobbyName, bool isPrivate)
    {
        GameEvents.OnCreateLobbyStarted?.Invoke();
        
        try
        {
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            };
            
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, Constants.MAX_PLAYER_AMOUNT, lobbyOptions);

            var relayJoinCode = await RelayManager.Instance.CreateRelayAsync();

            await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { Constants.KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            PlayerConnectionManager.Instance.StartHost();
            
            await Loader.Load(Scene.CharacterSelectScene, true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Lobby creation is failed! " + e);
            
            GameEvents.OnCreateLobbyFailed?.Invoke();
        }
    }

    public async UniTask QuickJoinAsync()
    {
        GameEvents.OnJoinStarted?.Invoke();
        
        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            var isRelayCreated = await RelayManager.Instance.JoinRelayAsync(_joinedLobby);
            
            if (isRelayCreated)
            {
                PlayerConnectionManager.Instance.StartClient();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Quick lobby join is failed! " + e);
            
            GameEvents.OnQuickJoinFailed?.Invoke();
        }
    }

    public async UniTask JoinWithCodeAsync(string lobbyCode)
    {
        GameEvents.OnJoinStarted?.Invoke();
        
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            
            var isRelayCreated = await RelayManager.Instance.JoinRelayAsync(_joinedLobby);
            
            if (isRelayCreated)
            {
                PlayerConnectionManager.Instance.StartClient();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Join lobby with code is failed! " + e);
            
            GameEvents.OnJoinFailed?.Invoke();
        }
    }
    
    public async UniTask JoinWithIdAsync(string lobbyId)
    {
        GameEvents.OnJoinStarted?.Invoke();
        
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            
            var isRelayCreated = await RelayManager.Instance.JoinRelayAsync(_joinedLobby);

            if (isRelayCreated)
            {
                PlayerConnectionManager.Instance.StartClient();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Join lobby with code is failed! " + e);
            
            GameEvents.OnJoinFailed?.Invoke();
        }
    }

    public async UniTask DeleteLobby()
    {
        if(_joinedLobby is null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Lobby cannot deleted! " + e);
        }
    }

    public async UniTask LeaveLobby()
    {
        if(_joinedLobby is null) return;
        
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Player cannot leave from the lobby! " + e);
        }
    }
    
    public async UniTask KickPlayer(string playerId)
    {
        if(IsLobbyHost)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Player cannot leave from the lobby! " + e);
            }
        }
    }

    public override async void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            await DeleteLobby();
        }
    }

    public override void OnDestroy()
    {
        DisposeUpdateToken();
    }

    private void DisposeUpdateToken()
    {
        _updateCancellationToken?.Cancel();
        _updateCancellationToken?.Dispose();
        _updateCancellationToken = null;
    }
}