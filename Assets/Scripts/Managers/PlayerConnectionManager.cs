using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerConnectionManager : NetworkBehaviour
{
    public static PlayerConnectionManager Instance { get; private set; }

    private NetworkList<PlayerData> _playerNetworkData;

    [SerializeField] private Color[] _playerColors;

    private bool _isServerDataRemoved;
    public string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "PlayerName" + Random.Range(100, 1000));
        set => PlayerPrefs.SetString("PlayerName", value);
    }

    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);

        _playerNetworkData = new NetworkList<PlayerData>();
    }

    private void OnEnable()
    {
        _playerNetworkData.OnListChanged += OnPlayerDataListChanged;
    }

    private void OnDisable()
    {
        _playerNetworkData.OnListChanged -= OnPlayerDataListChanged;
    }

    private void OnPlayerDataListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        GameEvents.OnPlayerDataNetworkListChanged?.Invoke();
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += Server_OnConnectionApprovalCallback;
        
        NetworkManager.Singleton.OnClientConnectedCallback += Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Server_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        GameEvents.OnTryingToJoinGame?.Invoke();
        
        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        
        NetworkManager.Singleton.StartClient();
    }
    
    private void Server_OnConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        var sceneName = SceneManager.GetActiveScene().name;
        
        var isPreperationScenesActive = sceneName == Scene.LobbyScene.ToString() ||
                                   sceneName == Scene.CharacterSelectScene.ToString();

        var isGameFull = NetworkManager.Singleton.ConnectedClientsIds.Count >= Constants.MAX_PLAYER_AMOUNT;

        if (!isPreperationScenesActive)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started!";
            return;
        }
        
        if (isGameFull)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full!";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }
    
    private void Server_OnClientConnectedCallback(ulong clientId)
    {
        var data = new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId(),
            playerName =  PlayerName,
            playerId = AuthenticationService.Instance.PlayerId
        };
        
        _playerNetworkData.Add(data);
    }

    public void DisconnectServer ()
    {
        if (_playerNetworkData.Contains(GetPlayerData()))
        {
            _isServerDataRemoved = true;
            
            _playerNetworkData.Remove(GetPlayerData());
        }
    }
    
    private void Server_OnClientDisconnectCallback(ulong clientId)
    {
        if(_isServerDataRemoved) return;
        
        for (int i = 0; i < _playerNetworkData.Count; i++)
        {
            var playerData = _playerNetworkData[i];

            if (playerData.clientId.Equals(clientId))
            {
                _playerNetworkData.RemoveAt(i);
            }
        }
    }

    private void Client_OnClientDisconnectCallback(ulong clientId)
    {
        GameEvents.OnFailedToJoinGame?.Invoke();
    }
    
    private void Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(PlayerName);
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        
        if(playerDataIndex < 0) return;

        var playerData = _playerNetworkData[playerDataIndex];
        playerData.playerName = playerName;
        _playerNetworkData[playerDataIndex] = playerData;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        
        if(playerDataIndex < 0) return;

        var playerData = _playerNetworkData[playerDataIndex];
        playerData.playerId = playerId;
        _playerNetworkData[playerDataIndex] = playerData;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (var playerData in _playerNetworkData)
        {
            if (playerData.clientId.Equals(clientId))
            {
                return playerData;
            }
        }

        return default;
    }
    
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerNetworkData.Count; i++)
        {
            if (_playerNetworkData[i].clientId.Equals(clientId))
            {
                return i;
            }
        }

        return -1;
    }
    
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.DisconnectClient(clientId);
        Server_OnClientDisconnectCallback(clientId);
    }
    
    public bool IsPlayerIndexConnected(int playerIndex) => playerIndex < _playerNetworkData.Count;
    public PlayerData GetPlayerData() => GetPlayerDataFromClientId(NetworkManager.LocalClientId);
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex) => _playerNetworkData[playerIndex];
    public Color GetPlayerColor(int colorId) => _playerColors[colorId];
    public void ChangePlayerColor(int colorId) => ChangePlayerColorServerRpc(colorId);
    

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if(!IsColorAvailable(colorId)) return;

        var playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        
        if(playerDataIndex < 0) return;

        var playerData = _playerNetworkData[playerDataIndex];
        playerData.colorId = colorId;
        _playerNetworkData[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (var playerData in _playerNetworkData)
        {
            if (playerData.colorId.Equals(colorId))
            {
                return false;
            }
        }

        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < _playerColors.Length; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }

        return -1;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            _isServerDataRemoved = true;
        }
        
        NetworkManager.ConnectionApprovalCallback -= Server_OnConnectionApprovalCallback;
        
        NetworkManager.OnClientConnectedCallback -= Server_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= Server_OnClientDisconnectCallback;
        
        NetworkManager.OnClientConnectedCallback -= Client_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= Client_OnClientDisconnectCallback;
    }
}