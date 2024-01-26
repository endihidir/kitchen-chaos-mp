using UnityEngine;
using UnityEngine.UI;

public class LobbyFadePanelUI : MonoBehaviour
{
    [SerializeField] private Image _image;
    
    private void OnEnable()
    {
        GameEvents.OnEnableLobbyFadePanel += OnEnableLobbyFadePanel;
    }

    private void OnDisable()
    {
        GameEvents.OnEnableLobbyFadePanel -= OnEnableLobbyFadePanel;
    }

    private void OnEnableLobbyFadePanel(bool enable)
    {
        _image.enabled = enable;
    }
}