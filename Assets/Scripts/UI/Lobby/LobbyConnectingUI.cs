using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyConnectingUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiHandler;
    [SerializeField] private Button _closeButton;
    
    private void Awake()=> Hide();
    
    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnClose);
        GameEvents.OnTryingToJoinGame += OnTryingToJoinGame;
        GameEvents.OnFailedToJoinGame += OnFailedToJoinGame;
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(OnClose);
        GameEvents.OnTryingToJoinGame -= OnTryingToJoinGame;
        GameEvents.OnFailedToJoinGame -= OnFailedToJoinGame;
    }

    private void OnClose()
    {
        Hide();
        
        //NetworkManager.Singleton.Shutdown();
    }

    private void OnFailedToJoinGame() => Hide();
    private void OnTryingToJoinGame() => Show();
    private void Show() => _uiHandler.SetActive(true);
    private void Hide() => _uiHandler.SetActive(false);
} 