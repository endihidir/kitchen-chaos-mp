using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] protected KitchenObjectSO _kitchenObjectSo;
    
   public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
   {
       if(otherKitchenObjectParent.HasKitchenObject) return;
 
       KitchenObjectSpawner.Instance.SpawnKitchenObject(_kitchenObjectSo, otherKitchenObjectParent);
       
       SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);

       InvokeStart();
   }
}