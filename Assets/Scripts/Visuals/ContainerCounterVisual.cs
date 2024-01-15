using Unity.Netcode;
using UnityEngine;

public class ContainerCounterVisual : BaseCounterVisual
{
    [SerializeField] private Animator _animator;
    
    private static readonly int OpenClose = Animator.StringToHash("OpenClose");

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        _animator = GetComponentInChildren<Animator>(true);
    }
#endif

    protected override void OnSelectedCounterChanged(IInteractable<BaseCounter> interactableCounter)
    {
        base.OnSelectedCounterChanged(interactableCounter);
        
        if (_interactableCounter == interactableCounter)
        {
            _interactableCounter.OnStart(TriggerAnimServerRpc);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TriggerAnimServerRpc()
    {
        TriggerAnimClientRpc();
    }

    [ClientRpc]
    private void TriggerAnimClientRpc()
    {
        _animator.SetTrigger(OpenClose);
    }
}
