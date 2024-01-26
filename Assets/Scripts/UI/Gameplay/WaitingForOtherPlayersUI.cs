using TMPro;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    [SerializeField] private GameObject _panelHandler;
    
    [SerializeField] private TextMeshProUGUI _waitForOtherPlayersTxt;

    private void Awake()
    {
        Show();
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
        GameEvents.OnMultiplayerGamePaused += OnLocalGamePaused;
        GameEvents.OnPlayerReadyToPlay += OnPlayerReadyToPlay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
        GameEvents.OnMultiplayerGamePaused -= OnLocalGamePaused;
        GameEvents.OnPlayerReadyToPlay -= OnPlayerReadyToPlay;
    }

    private void OnPlayerReadyToPlay()
    {
        _waitForOtherPlayersTxt.text = "WAITING FOR OTHER PLAYERS...";
    }

    private void OnLocalGamePaused(bool isLocalGamePaused)
    {
        
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        if (gamePlayState == GamePlayState.CountdownToStart)
        {
            Hide();
        }
    }
    
    private void Show()
    {
        _panelHandler.SetActive(true);
    }

    private void Hide()
    {
        _panelHandler.SetActive(false);
    }
}