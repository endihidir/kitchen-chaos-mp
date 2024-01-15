using Unity.Netcode;
using UnityEngine;

public class DeliveryCounterVisual : BaseCounterVisual
{
    [SerializeField] private DeliveryFeedbackUI _deliveryFeedbackUI;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        _deliveryFeedbackUI = GetComponentInChildren<DeliveryFeedbackUI>(true);
    }
#endif

    protected override void Start()
    {
        base.Start();
        
        _interactableCounter.OnStart(DeliverySuccess).OnInterrupt(DeliveryFail);
    }
    
    private void DeliverySuccess()
    {
        DeliverySuccessServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverySuccessServerRpc()
    {
        DeliverySuccessClientRpc();
    }
    
    [ClientRpc]
    private void DeliverySuccessClientRpc()
    {
        _deliveryFeedbackUI.PlaySuccess();
    }
    
    private void DeliveryFail()
    {
        DeliveryFailServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeliveryFailServerRpc()
    {
        DeliveryFailClientRpc();
    }
    
    [ClientRpc]
    private void DeliveryFailClientRpc()
    {
        _deliveryFeedbackUI.PlayFail();
    }
}