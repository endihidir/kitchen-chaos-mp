using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CuttingRecipe")]
public class CuttingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float cutProgressTime = 2f;
}
