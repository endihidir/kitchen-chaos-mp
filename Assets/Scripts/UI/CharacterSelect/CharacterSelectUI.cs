using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton, _readyButton;

    [SerializeField] private TextMeshProUGUI _lobbyNameText, _lobbyCodeText;
    
    private void OnEnable()
    {
        _readyButton.onClick.AddListener(OnReady);
        _mainMenuButton.onClick.AddListener(OnClickMainMenu);
    }

    private void OnDisable()
    {
        _readyButton.onClick.RemoveListener(OnReady);
        _mainMenuButton.onClick.RemoveListener(OnClickMainMenu);
    }

    private void Start()
    {
        var currentLobby = LobbyManager.Instance.Lobby;
        
        _lobbyNameText.text = "Lobby Name: " + currentLobby.Name;
        _lobbyCodeText.text = "Lobby Code: " + currentLobby.LobbyCode;
    }

    private void OnReady()
    {
        PlayerSelectReady.Instance.SetPlayerReady();
    }

    private async void OnClickMainMenu()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerConnectionManager.Instance.DisconnectServer();
            
            await LobbyManager.Instance.DeleteLobby();
        }
        else
        {
            await LobbyManager.Instance.LeaveLobby();
            
            //PlayerSelectReady.Instance.SetPlayerReady();
        }

        NetworkManager.Singleton.Shutdown();

        await Loader.Load(Scene.MainMenuScene);
    }
}
