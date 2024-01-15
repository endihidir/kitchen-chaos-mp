using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiHandler;

    [SerializeField] private Button _closeButton;
    
    [SerializeField] private TextMeshProUGUI _messageTxt;
    
    private void Awake() => Hide();
    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnClick);
        GameEvents.OnFailedToJoinGame += OnFailedToJoinGame;
        
        GameEvents.OnCreateLobbyStarted += OnCreateLobbyStarted;
        GameEvents.OnCreateLobbyFailed += OnCreateLobbyFailed;
        
        GameEvents.OnJoinStarted += OnJoinStarted;
        GameEvents.OnJoinFailed += OnJoinFailed;
        GameEvents.OnQuickJoinFailed += OnQuickJoinFailed;
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(OnClick);
        GameEvents.OnFailedToJoinGame -= OnFailedToJoinGame;
        
        GameEvents.OnCreateLobbyStarted -= OnCreateLobbyStarted;
        GameEvents.OnCreateLobbyFailed -= OnCreateLobbyFailed;
        
        GameEvents.OnJoinStarted -= OnJoinStarted;
        GameEvents.OnJoinFailed -= OnJoinFailed;
        GameEvents.OnQuickJoinFailed -= OnQuickJoinFailed;
    }
    
    private void OnClick()
    {
        Hide();
        
        NetworkManager.Singleton.Shutdown();
    }

    private void OnFailedToJoinGame()
    {
        var reason = NetworkManager.Singleton.DisconnectReason;
        
        var message = reason.Equals("") ? "Failed to connect!" : reason;
        
        ShowMessage(message);
    }
    
    private void OnCreateLobbyStarted() => ShowMessage("Creating Lobby...", false);
    private void OnCreateLobbyFailed() => ShowMessage("Failed to create Lobby!");
    private void OnJoinStarted() => ShowMessage("Joining Lobby...", false);
    private void OnJoinFailed() => ShowMessage("Failed to join Lobby!");
    private void OnQuickJoinFailed() => ShowMessage("Could not find a Lobby to Quick Join!");

    private void ShowMessage(string message, bool isFailMessage = true)
    {
        Show();
        
        _messageTxt.text = message;

        _messageTxt.color = isFailMessage ? Color.red : Color.white;
    }

    private void Show() => _uiHandler.SetActive(true);
    private void Hide() => _uiHandler.SetActive(false);
}