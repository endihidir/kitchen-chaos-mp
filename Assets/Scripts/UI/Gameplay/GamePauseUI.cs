using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private GameObject _gamePausePanel;

    [SerializeField] private Button _mainMenuButton, _resumeButton, _optionsButton;

    [SerializeField] private OptionsUI _optionsUI;

    private void OnEnable()
    {
        GameEvents.OnLocalGamePaused += OnLocalGamePaused;
        
        _mainMenuButton.onClick.AddListener(OnClickMainMenuButton);
        
        _resumeButton.onClick.AddListener(OnClickResumeButton);
        
        _optionsButton.onClick.AddListener(OnClickOptionsButton);
    }

    private void OnDisable()
    {
        GameEvents.OnLocalGamePaused -= OnLocalGamePaused;
        
        _mainMenuButton.onClick.RemoveListener(OnClickMainMenuButton);
        
        _resumeButton.onClick.RemoveListener(OnClickResumeButton);
        
        _optionsButton.onClick.RemoveListener(OnClickOptionsButton);
    }

    private void OnLocalGamePaused(bool isLocalGamePaused)
    {
        _gamePausePanel.SetActive(isLocalGamePaused);

        if (isLocalGamePaused)
        {
            _resumeButton.Select();
        }
        else
        {
            _optionsUI.SetActivePanel(false);
        }
    }
    
    private async void OnClickMainMenuButton()
    {
        await Loader.Load(Scene.MainMenuScene);
        
        Time.timeScale = 1;
    }

    private void OnClickResumeButton()
    {
        GameEvents.OnGamePause?.Invoke();
    }

    private void OnClickOptionsButton()
    {
        _optionsUI.SetActivePanel(true);
    }
}