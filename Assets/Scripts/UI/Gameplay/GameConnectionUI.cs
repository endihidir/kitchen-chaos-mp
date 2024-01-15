using UnityEngine;
using UnityEngine.UI;

public class GameConnectionUI : MonoBehaviour
{
    [SerializeField] private Button _playAgainButton;

    [SerializeField] private GameObject _hostDisconnectedPanel;

    private void OnEnable()
    {
        _playAgainButton.onClick.AddListener(OnClickPlayAgainButton);
        GameEvents.OnFailedToJoinGame += OnFailedToJoinGame;
    }

    private void OnDisable()
    {
        _playAgainButton.onClick.RemoveListener(OnClickPlayAgainButton);
        GameEvents.OnFailedToJoinGame -= OnFailedToJoinGame;
    }

    private async void OnClickPlayAgainButton()
    {
        _hostDisconnectedPanel.SetActive(false);
        
        await Loader.Load(Scene.MainMenuScene);
    }
    
    private void OnFailedToJoinGame()
    {
        _hostDisconnectedPanel.SetActive(true);
    }
}