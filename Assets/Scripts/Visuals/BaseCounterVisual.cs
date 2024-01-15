using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounterVisual : NetworkBehaviour
{
    protected IInteractable<BaseCounter> _interactableCounter;
    
    [SerializeField] private Material _defaultMaterial, _selectedMaterial;
    
    [SerializeField] private Tag_Renderer[] _visualMeshRenderers;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (Application.isPlaying) return;

        _visualMeshRenderers = GetComponentsInChildren<Tag_Renderer>(true);
    }
#endif

    protected virtual void Awake()
    {
        _interactableCounter = GetComponent<IInteractable<BaseCounter>>();
    }

    protected virtual void OnEnable()
    {
        GameEvents.OnSelectedCounterChanged += OnSelectedCounterChanged;
    }

    protected virtual void OnDisable()
    {
        GameEvents.OnSelectedCounterChanged -= OnSelectedCounterChanged;
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void OnSelectedCounterChanged(IInteractable<BaseCounter> interactableCounter)
    {
        for (int i = 0; i < _visualMeshRenderers.Length; i++)
        {
            _visualMeshRenderers[i].MeshRenderer.material = interactableCounter == _interactableCounter ? _selectedMaterial : _defaultMaterial;
        }
    }
}