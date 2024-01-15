using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/FryingRecipe")]
public class FryingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float fryProgressTime = 8f;
}
