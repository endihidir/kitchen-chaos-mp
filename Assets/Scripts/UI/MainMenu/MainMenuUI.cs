using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _playeSinglePlayerButton, _playMultiplayerButton, _quitButton;
    
    private void OnEnable()
    {
        _playMultiplayerButton.onClick.AddListener(()=> OnClickPlayButton(true));
        
        _playeSinglePlayerButton.onClick.AddListener(()=> OnClickPlayButton(false));
        
        _quitButton.onClick.AddListener(OnClickQuitButton);
        _playeSinglePlayerButton.Select();
    }

    private void OnDisable()
    {
        _playMultiplayerButton.onClick.RemoveListener(()=> OnClickPlayButton(true));
        
        _playeSinglePlayerButton.onClick.RemoveListener(()=> OnClickPlayButton(false));
        
        _quitButton.onClick.RemoveListener(OnClickQuitButton);
    }
    
    private async void OnClickPlayButton(bool isMultiplayer)
    {
        LobbyManager.playMultiplayer = isMultiplayer;
        
        await Loader.Load(Scene.LobbyScene);
    } 
    
    private void OnClickQuitButton()
    {
        Application.Quit();
    }
}