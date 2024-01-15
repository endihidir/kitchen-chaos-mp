using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _deliveryNameTxt;
    
    [SerializeField] private Image _deliveryIconPrefab;

    [SerializeField] private Transform _deliveryLayoutGroupT;
    public bool IsActive => gameObject.activeSelf;

    private List<Image> _recipeIcons = new List<Image>();
    
    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            PrepareNewRecipeIcon();
        }
    }

    public void PrepareRecipes(RecipeSO recipeSo)
    {
        gameObject.name = recipeSo.recipeName;
        _deliveryNameTxt.text = recipeSo.recipeName;

        var recipeCount = recipeSo.kitchenObjectSoArray.Length;
        var extraRecipeCount = recipeCount > _recipeIcons.Count ? recipeCount - _recipeIcons.Count : 0;
        
        for (int i = 0; i < extraRecipeCount; i++)
        {
            PrepareNewRecipeIcon();
        }

        for (int i = 0; i < recipeCount; i++)
        {
            _recipeIcons[i].enabled = true;
            _recipeIcons[i].sprite = recipeSo.kitchenObjectSoArray[i].sprite;
        }       
    }

    public void DisableAllRecipeIcons()
    {
        for (int i = 0; i < _recipeIcons.Count; i++)
        {
            _recipeIcons[i].sprite = null;
            _recipeIcons[i].enabled = false;
        }
    }
    
    private void PrepareNewRecipeIcon()
    {
        var recipeIcon = Instantiate(_deliveryIconPrefab, _deliveryLayoutGroupT);
        recipeIcon.enabled = false;
        _recipeIcons.Add(recipeIcon);
    }
}