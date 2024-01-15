using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Transform _gameOverUIHandler;
    [SerializeField] private TextMeshProUGUI _deliveredRecipeCountTxt;

    private int _recipeCounter;
    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
        GameEvents.OnDeliverRecipe += OnDeliverRecipe;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
        GameEvents.OnDeliverRecipe -= OnDeliverRecipe;
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        if (gamePlayState == GamePlayState.WaitingToStart)
        {
            _recipeCounter = 0;
        }
        
        _gameOverUIHandler.gameObject.SetActive(gamePlayState == GamePlayState.GameOver);   
    }
    
    private void OnDeliverRecipe(RecipeSO recipeSo)
    {
        _recipeCounter++;

        _deliveredRecipeCountTxt.text = _recipeCounter.ToString("0");
    }
}
