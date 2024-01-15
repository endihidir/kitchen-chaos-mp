using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        
        if (LobbyManager.Instance)
        {
            Destroy(LobbyManager.Instance.gameObject);   
        }

        if (PlayerConnectionManager.Instance)
        {
            Destroy(PlayerConnectionManager.Instance.gameObject);
        }

        if (RelayManager.Instance)
        {
            Destroy(RelayManager.Instance.gameObject);
        }
    }
}
