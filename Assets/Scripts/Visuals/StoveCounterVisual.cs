using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

public class StoveCounterVisual : ProgressCounterVisual
{
    [SerializeField] private GameObject[] _visualObjects;

    protected override void OnDropOrSwap()
    {
        base.OnDropOrSwap();
        
        EnableProgressBarClientRpc(true);
    }
    
    protected override void TakeObject()
    {
        base.TakeObject();
        
        EnableProgressBarClientRpc(false);
    }
    
    [ClientRpc]
    private void EnableProgressBarClientRpc(bool enable)
    {
        _visualObjects.ForEach(x => x.SetActive(enable));
    }
}