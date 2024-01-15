using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countDownTxt;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
        GameEvents.OnCountDownUpdate += OnCountDownUpdate;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
        GameEvents.OnCountDownUpdate -= OnCountDownUpdate;
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        _countDownTxt.gameObject.SetActive(gamePlayState == GamePlayState.CountdownToStart);
    }
    
    private void OnCountDownUpdate(float timer)
    {
        _countDownTxt.text = timer.ToString("0");
    }
}