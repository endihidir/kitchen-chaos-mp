using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private NetworkObject _playerPrefab;
    
    [SerializeField] private NetworkVariable<GamePlayState> _currentGameplayState = new NetworkVariable<GamePlayState>(GamePlayState.WaitingToStart);

    private NetworkVariable<float> _countDownStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> _gameOverTimer = new NetworkVariable<float>(120f);
    private NetworkVariable<bool> _isMultiplayerGamePaused = new NetworkVariable<bool>(false);

    private CancellationTokenSource _cancellationTokenSource;

    private bool _isLocalGamePaused;
    
    private Dictionary<ulong, bool> _playerReadyDictionary = new Dictionary<ulong, bool>();
    private Dictionary<ulong, bool> _playerPauseDictionary = new Dictionary<ulong, bool>();
    public GamePlayState CurrentGameplayState => _currentGameplayState.Value;
    public bool IsWaitingToStart => _currentGameplayState.Value == GamePlayState.WaitingToStart;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameEvents.OnGameStateChanged?.Invoke(_currentGameplayState.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        GameEvents.OnGamePause += OnPause;
        GameInput.Instance.OnInteractAction += OnInteractAction;
        
        _currentGameplayState.OnValueChanged += OnGameplayStateChanged;
        _countDownStartTimer.OnValueChanged += OnCountDownChanged;
        _gameOverTimer.OnValueChanged += OnGameOverTimerChanged;
        _isMultiplayerGamePaused.OnValueChanged += OnGamePauseValueChanged;

        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        OnStartUpdate();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameEvents.OnGamePause -= OnPause;
        GameInput.Instance.OnInteractAction -= OnInteractAction;
        
        _currentGameplayState.OnValueChanged -= OnGameplayStateChanged;
        _countDownStartTimer.OnValueChanged -= OnCountDownChanged;
        _gameOverTimer.OnValueChanged -= OnGameOverTimerChanged;
        _isMultiplayerGamePaused.OnValueChanged -= OnGamePauseValueChanged;

        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }
    }

    private void OnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        foreach (var clientsId in NetworkManager.ConnectedClientsIds)
        {
            var playerClone = Instantiate(_playerPrefab);
            playerClone.SpawnAsPlayerObject(clientsId, true);
        }
    }

    private void OnGameplayStateChanged(GamePlayState previousvalue, GamePlayState newvalue)
    {
        GameEvents.OnGameStateChanged?.Invoke(newvalue);
    }
    
    private void OnCountDownChanged(float previousvalue, float newvalue)
    {
        GameEvents.OnCountDownUpdate?.Invoke(newvalue);
    }

    private void OnGameOverTimerChanged(float previousvalue, float newvalue)
    {
        var mapVal = Map(newvalue, 120f, 1f, 0f, 1f);
                    
        GameEvents.OnGetGameTimer?.Invoke(mapVal);
    }
    
    private void OnGamePauseValueChanged(bool previousvalue, bool newvalue)
    {
        Time.timeScale = newvalue ? 0f : 1f;
        
        GameEvents.OnMultiplayerGamePaused?.Invoke(newvalue);
    }

    private void OnInteractAction(object sender, EventArgs e)
    {
        if (_currentGameplayState.Value == GamePlayState.WaitingToStart)
        {
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        
        PlayerReadyForPlayClientRpc(serverRpcParams.Receive.SenderClientId);

        var allClientsReady = true;
        
       foreach (var clientsId in NetworkManager.ConnectedClientsIds)
       {
           if (!_playerReadyDictionary.ContainsKey(clientsId) || !_playerReadyDictionary[clientsId])
           {
               allClientsReady = false;
               break;
           }
       }

       if (allClientsReady)
       {
           _currentGameplayState.Value = GamePlayState.CountdownToStart;
       }
    }

    [ClientRpc]
    private void PlayerReadyForPlayClientRpc(ulong senderClientId)
    {
        var playerConnectionManager = PlayerConnectionManager.Instance;

        var isPlayerContains = playerConnectionManager.GetPlayerData().clientId.Equals(senderClientId);

        if (isPlayerContains)
        {
            GameEvents.OnPlayerReadyToPlay?.Invoke();
        }
    }

    private void OnStartUpdate()
    {
        if(!IsServer) return;
        
        UpdateAsync();
    }

    private void OnPause()
    {
        if (_currentGameplayState.Value is GamePlayState.WaitingToStart or GamePlayState.GameOver) return;

        _isLocalGamePaused = !_isLocalGamePaused;
            
        if (_isLocalGamePaused)
        {
            PauseGameServerRpc();
        }
        else
        {
            UnpauseGameServerRpc();
        }
        
        GameEvents.OnLocalGamePaused?.Invoke(_isLocalGamePaused);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
        
        ChangeGamePausedState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;
        
        ChangeGamePausedState();
    }

    private void ChangeGamePausedState()
    {
        foreach (var clientsId in NetworkManager.ConnectedClientsIds)
        {
            if (_playerPauseDictionary.ContainsKey(clientsId) && _playerPauseDictionary[clientsId])
            {
                _isMultiplayerGamePaused.Value = true;
                return; 
            }
        }

        _isMultiplayerGamePaused.Value = false;
    }

    private async void UpdateAsync()
    {
        _cancellationTokenSource ??= new CancellationTokenSource();
        
        while (_cancellationTokenSource is not null)
        {
            switch (_currentGameplayState.Value)
            {
                case GamePlayState.WaitingToStart:
                    
                    var isCancelled = await Delay(0);
                    
                    if(isCancelled) return;

                    break;
                case GamePlayState.CountdownToStart:
                    
                    SoundManager.Instance.PlaySound(SoundType.Warning, 1, false);
                    
                    isCancelled = await Delay(1000);

                    if(isCancelled) return;
                    
                    _countDownStartTimer.Value -= 1f;

                    if (_countDownStartTimer.Value < 1f)
                    {
                        _currentGameplayState.Value = GamePlayState.GamePlay;
                        _countDownStartTimer.Value = 3f;
                    }

                    break;
                case GamePlayState.GamePlay:

                    isCancelled = await Delay(1000);
                    
                    if(isCancelled) return;

                    _gameOverTimer.Value -= 1f;

                    if (_gameOverTimer.Value < 1f)
                    {
                        _currentGameplayState.Value = GamePlayState.GameOver;
                        _gameOverTimer.Value = 120f;
                    }
                    
                    break;
                case GamePlayState.GameOver:

                    DisposeToken();
                    
                    break;
            }
        }
    }

    private async UniTask<bool> Delay(int milliseconds)
    {
        try
        {
            await UniTask.Delay(milliseconds, DelayType.DeltaTime, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            return false;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("State Change Task Cancelled!");
            return true;
        }
    }

    private void DisposeToken()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        DisposeToken();
    }
    
    private float Map(float input, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return outputMin + (input - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
}