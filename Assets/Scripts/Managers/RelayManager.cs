using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class RelayManager : NetworkBehaviour
{
    public static RelayManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        
        DontDestroyOnLoad(gameObject);

    }
    
    public async UniTask<string> CreateRelayAsync()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(Constants.MAX_PLAYER_AMOUNT - 1);
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.Log(e);

            return default;
        }
    }
    
    public async UniTask<bool> JoinRelayAsync(Lobby joinedLobby)
    {
        if (joinedLobby.Data is not null)
        {
            var relayJoinCode = joinedLobby.Data[Constants.KEY_RELAY_JOIN_CODE].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            return true;
        }
        else
        {
            GameEvents.OnJoinFailed?.Invoke();

            return false;
        }
    }
}
