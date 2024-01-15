using System.Collections.Generic;
using Unity.Netcode;
public class PlayerSelectReady : NetworkBehaviour
{
    public static PlayerSelectReady Instance { get; private set; }
    
    private Dictionary<ulong, bool> _playerReadyDictionary = new Dictionary<ulong, bool>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        
        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

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
            LoadGameScene();
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        _playerReadyDictionary[clientId] = true;
        
        GameEvents.OnPlayerReadyChanged?.Invoke();
    }

    private async void LoadGameScene()
    {
        await LobbyManager.Instance.DeleteLobby();
        
        await Loader.Load(Scene.GameScene, true);
    }

    public bool IsPlayerReady(ulong clientId) => _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
}