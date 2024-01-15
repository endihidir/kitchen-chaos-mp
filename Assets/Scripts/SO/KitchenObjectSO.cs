using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{
    public int id;
    
    public string objectName;
    
    public KitchenObject prefab;

    public Sprite sprite;
}