using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _gamePausePanel;

    private bool _isLocalGamePaused, _isMultiplayerGamePaused;

    private void OnEnable()
    {
        GameEvents.OnMultiplayerGamePaused += OnMultiplayerGamePaused;
        GameEvents.OnLocalGamePaused += OnLocalGamePaused;
    }

    private void OnDisable()
    {
        GameEvents.OnMultiplayerGamePaused -= OnMultiplayerGamePaused;
        GameEvents.OnLocalGamePaused -= OnLocalGamePaused;
    }

    private void OnMultiplayerGamePaused(bool isMultiplayerPaused)
    {
        _isMultiplayerGamePaused = isMultiplayerPaused;
        
        if (_isLocalGamePaused)
        {
            _gamePausePanel.SetActive(false);
            return;
        }
        
        _gamePausePanel.SetActive(_isMultiplayerGamePaused);
    }
    
    private void OnLocalGamePaused(bool isLocalGamePaused)
    {
        _isLocalGamePaused = isLocalGamePaused;

        if (_isLocalGamePaused && _isMultiplayerGamePaused)
        {
            _gamePausePanel.SetActive(false);
        }
        else
        {
            _gamePausePanel.SetActive(true);
        }
    }
}