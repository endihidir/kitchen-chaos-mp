using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int _playerIndex;
    
    [SerializeField] private GameObject _playerVisualObject, _readyTextObject;

    [SerializeField] private TextMeshPro _playerNameText;

    [SerializeField] private PlayerVisual _playerVisual;

    [SerializeField] private Button _kickButton;

#if UNITY_EDITOR

    private void OnValidate()
    {
        _playerVisual = GetComponentInChildren<PlayerVisual>(true);
    }
    
#endif

    private void OnEnable()
    {
        GameEvents.OnPlayerDataNetworkListChanged += OnPlayerDataNetworkListChanged;

        GameEvents.OnPlayerReadyChanged += OnPlayerReadyChanged;
        
        _kickButton.onClick.AddListener(OnKick);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDataNetworkListChanged -= OnPlayerDataNetworkListChanged;
        
        GameEvents.OnPlayerReadyChanged -= OnPlayerReadyChanged;
        
        _kickButton.onClick.RemoveListener(OnKick);
    }

    private void Start()
    {
        UpdatePlayer();
    }

    private void OnPlayerDataNetworkListChanged()
    {
        UpdatePlayer();
    }
    
    private void OnPlayerReadyChanged()
    {
        UpdatePlayer();
    }
    
    private async void OnKick()
    {
        var playerData = PlayerConnectionManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);

        PlayerConnectionManager.Instance.KickPlayer(playerData.clientId);
        
        await LobbyManager.Instance.KickPlayer(playerData.playerId.ToString());
    }

    private void UpdatePlayer()
    {
        var showPlayer = PlayerConnectionManager.Instance.IsPlayerIndexConnected(_playerIndex);
        
        _playerVisualObject.SetActive(showPlayer);
        _playerNameText.gameObject.SetActive(showPlayer);
        _kickButton.gameObject.SetActive(showPlayer && NetworkManager.Singleton.IsServer);

        if (showPlayer)
        {
            var playerData = PlayerConnectionManager.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
            var isPlayerReady = PlayerSelectReady.Instance.IsPlayerReady(playerData.clientId);
            _playerNameText.text = playerData.playerName.ToString();
            _readyTextObject.SetActive(isPlayerReady);
            
            SetPlayerVisualBy(playerData.colorId);
        }
    }

    public void SetPlayerVisualBy(int colorIndex)
    {
        var color = PlayerConnectionManager.Instance.GetPlayerColor(colorIndex);
        _playerVisual.SetPlayerColor(color);
    }
    
}