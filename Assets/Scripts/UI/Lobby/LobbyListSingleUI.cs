using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private Button _lobbyButton;
    
    private Lobby _lobby;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _lobbyButton = GetComponent<Button>();
    }
#endif
    private void OnEnable()
    {
        _lobbyButton.onClick.AddListener(OnTryJoinLobby);
    }

    private void OnDisable()
    {
        _lobbyButton.onClick.RemoveListener(OnTryJoinLobby);
    }

    private async void OnTryJoinLobby()
    {
        await LobbyManager.Instance.JoinWithIdAsync(_lobby.Id);
    }

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNameTxt.text = _lobby.Name;
    }
}