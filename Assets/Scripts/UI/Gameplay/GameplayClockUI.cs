using UnityEngine;
using UnityEngine.UI;

public class GameplayClockUI : MonoBehaviour
{
    [SerializeField] private Image _timerImage;
    [SerializeField] private GameObject _handler;
    
    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
        
        GameEvents.OnGetGameTimer += OnGetGameTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
        
        GameEvents.OnGetGameTimer -= OnGetGameTimer;
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        _handler.SetActive(gamePlayState == GamePlayState.GamePlay);
    }

    private void OnGetGameTimer(float timer)
    {
        _timerImage.fillAmount = timer;
    }
}
