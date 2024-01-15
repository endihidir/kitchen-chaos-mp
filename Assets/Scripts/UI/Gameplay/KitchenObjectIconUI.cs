using UnityEngine;
using UnityEngine.UI;

public class KitchenObjectIconUI : MonoBehaviour
{
    [SerializeField] private Image _kitchenObjectIcon;

    public void SetIcon(Sprite sprite)
    {
        _kitchenObjectIcon.sprite = sprite;
    }
    
}
