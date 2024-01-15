using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _lobbyNameInputField, _lobbyCodeInputField, _playerNameInputField;

    [SerializeField] private Toggle _lobbyPrivateToggle;
    
    [SerializeField] private Button _createLobbyButton, _quickJoinButton, _mainMenuButton, _joinCodeButton;

    [SerializeField] private Transform _lobbyContainer;

    [SerializeField] private LobbyListSingleUI _lobbyTemplate;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _lobbyTemplate = GetComponentInChildren<LobbyListSingleUI>(true);
    }
#endif
    
    private void Awake()
    {
        _lobbyPrivateToggle.isOn = false;
        
        _lobbyTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _createLobbyButton.onClick.AddListener(OnCreateGame);
        _quickJoinButton.onClick.AddListener(OnJoinGame);
        _mainMenuButton.onClick.AddListener(OnClickMainMenuButton);
        _joinCodeButton.onClick.AddListener(OnJoinCode);
        
        _playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
        
        GameEvents.OnLobbyListChanged += OnLobbyListChanged;
    }

    private void OnDisable()
    {
        _createLobbyButton.onClick.RemoveListener(OnCreateGame);
        _quickJoinButton.onClick.RemoveListener(OnJoinGame);
        _mainMenuButton.onClick.RemoveListener(OnClickMainMenuButton);
        _joinCodeButton.onClick.RemoveListener(OnJoinCode);
        
        _playerNameInputField.onValueChanged.RemoveListener(OnPlayerNameChanged);
        
        GameEvents.OnLobbyListChanged -= OnLobbyListChanged;
    }

    private void Start()
    {
        _playerNameInputField.text = PlayerConnectionManager.Instance.PlayerName;
    }

    private async void OnClickMainMenuButton()
    {
        await LobbyManager.Instance.LeaveLobby();

        await Loader.Load(Scene.MainMenuScene);
    }

    private async void OnCreateGame()
    {
        await LobbyManager.Instance.CreateLobbyAsync(_lobbyNameInputField.text, _lobbyPrivateToggle.isOn);
    }

    private async void OnJoinGame()
    {
        await LobbyManager.Instance.QuickJoinAsync();
    }
    
    private async void OnJoinCode()
    {
        await LobbyManager.Instance.JoinWithCodeAsync(_lobbyCodeInputField.text);
    }
    
    private void OnPlayerNameChanged(string name)
    {
        PlayerConnectionManager.Instance.PlayerName = name;
    }

    private void OnLobbyListChanged(List<Lobby> lobbyList)
    {
        UpdateLobbyList(lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in _lobbyContainer)
        {
            if(child.Equals(_lobbyTemplate.transform)) continue;
            Destroy(child.gameObject);
        }

        foreach (var lobby in lobbyList)
        {
            var lobbyListSingleUI = Instantiate(_lobbyTemplate, _lobbyContainer);
            lobbyListSingleUI.gameObject.SetActive(true);
            lobbyListSingleUI.SetLobby(lobby);
        }
    }
}