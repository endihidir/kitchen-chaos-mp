using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private DeliveryUI _deliveryUIPrefab;

    [SerializeField] private Transform _deliveryContainer, _deliveryUIPool, _titleT;

    private List<DeliveryUI> _spawnedDeliveries = new List<DeliveryUI>();

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            PrepareNewDeliveryUI();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += OnGameStateChanged;
        GameEvents.OnSpawnRecipe += OnSpawnRecipe;
        GameEvents.OnDeliverRecipe += OnDeliverRecipe;
    }
    
    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= OnGameStateChanged;
        GameEvents.OnSpawnRecipe -= OnSpawnRecipe;
        GameEvents.OnDeliverRecipe -= OnDeliverRecipe;
    }

    private void OnGameStateChanged(GamePlayState gamePlayState)
    {
        _titleT.gameObject.SetActive(gamePlayState == GamePlayState.GamePlay);
        _deliveryContainer.gameObject.SetActive(gamePlayState == GamePlayState.GamePlay);
    }

    private void OnSpawnRecipe(RecipeSO spawnedRecipeSo)
    {
        var delivery = _spawnedDeliveries.FirstOrDefault(x => !x.IsActive);

        if (!delivery)
        {
            delivery = PrepareNewDeliveryUI();
        }
        
        delivery.transform.SetParent(_deliveryContainer);
        
        delivery.PrepareRecipes(spawnedRecipeSo);
        
        delivery.gameObject.SetActive(true);
    }
    
    private void OnDeliverRecipe(RecipeSO deliveredRecipeSo)
    {
        var deliveredRecipe = _spawnedDeliveries.FirstOrDefault(x => x.name.Equals(deliveredRecipeSo.recipeName));
        
        if(!deliveredRecipe) return;
        
        deliveredRecipe.DisableAllRecipeIcons();

        deliveredRecipe.gameObject.SetActive(false);
        
        deliveredRecipe.transform.SetParent(_deliveryUIPool);
    }
    
    private DeliveryUI PrepareNewDeliveryUI()
    {
        var delivery = Instantiate(_deliveryUIPrefab, _deliveryUIPool);

        delivery.gameObject.SetActive(false);

        _spawnedDeliveries.Add(delivery);

        return delivery;
    }
}