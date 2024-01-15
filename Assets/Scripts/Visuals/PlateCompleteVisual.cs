using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [SerializeField] private GameObject[] _plateObjects;
    
    [SerializeField] private KitchenObjectIconUI _kitchenObjectIconPrefab;

    [SerializeField] private Transform _iconsHandler;

    private Camera _cam;
    private void Awake()
    {
        _cam = Camera.main;

        _plateObjects.ForEach(x => x.SetActive(false));
    }

    private void Update()
    {
        _iconsHandler.forward = _cam.transform.forward;
    }

    public void CreateVisual(KitchenObjectSO kitchenObjectSo)
    {
        _plateObjects.FirstOrDefault(x=> !x.activeSelf ? x.name.Equals(kitchenObjectSo.objectName) : default)?.SetActive(true);
        
        var kitchenObjectIcon = Instantiate(_kitchenObjectIconPrefab, _iconsHandler);
        
        kitchenObjectIcon.SetIcon(kitchenObjectSo.sprite);
    }
}