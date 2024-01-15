using UnityEngine;
using UnityEngine.UI;

public class ColorButtonUI : MonoBehaviour
{
    [field: SerializeField] public int ColorId { get; set; }
    
    [SerializeField] private Button _button;
    
    [SerializeField] private Image _colorImage, _selectedImage;

    private PlayerConnectionManager _playerConnectionManager;
    
    private void OnEnable()
    {
        GameEvents.OnPlayerDataNetworkListChanged += OnPlayerDataNetworkListChanged;
        
        _button.onClick.AddListener(OnClickColorButton);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDataNetworkListChanged -= OnPlayerDataNetworkListChanged;
        
        _button.onClick.RemoveListener(OnClickColorButton);
    }

    private void OnPlayerDataNetworkListChanged()
    {
        UpdateIsSelected();
    }

    private void Start()
    {
        _playerConnectionManager = PlayerConnectionManager.Instance;
        
        _colorImage.color = _playerConnectionManager.GetPlayerColor(ColorId);
        
        UpdateIsSelected();
    }

    private void OnClickColorButton()
    {
        _playerConnectionManager.ChangePlayerColor(ColorId);
    }

    private void UpdateIsSelected()
    {
        var isColorSelected = _playerConnectionManager.GetPlayerData().colorId == ColorId;
        
        _selectedImage.enabled = isColorSelected;
    }
}
