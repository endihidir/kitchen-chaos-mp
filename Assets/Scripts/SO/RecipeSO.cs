using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    
    public KitchenObjectSO[] kitchenObjectSoArray;
}
