using Unity.Netcode;

public class ClearCounter : BaseCounter, ISwappable
{
   public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
   {
      if (HasKitchenObject && otherKitchenObjectParent.HasKitchenObject)
      {
         if (otherKitchenObjectParent.KitchenObject is PlateKitchenObject playerPlateObject && playerPlateObject.TryAddIngredient(this))
         {
            SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);
            KitchenObject.DestroySelf();
         }
         else if (KitchenObject is PlateKitchenObject counterPlateObject && counterPlateObject.TryAddIngredient(otherKitchenObjectParent))
         {
            SoundManager.Instance.PlaySound(SoundType.ObjectDrop, 0.35f);
            otherKitchenObjectParent.KitchenObject.DestroySelf();
         }
         else
         {
            SwapIngredientsServerRpc(otherKitchenObjectParent.GetNetworkObject());
         }
      }
      else if(otherKitchenObjectParent.HasKitchenObject)
      {
         SoundManager.Instance.PlaySound(SoundType.ObjectDrop, 0.35f);
         otherKitchenObjectParent.KitchenObject.SetKitchenObjectParent(this);
      }
      else if (HasKitchenObject)
      {
         SoundManager.Instance.PlaySound(SoundType.ObjectPickUp, 0.35f);
         KitchenObject.SetKitchenObjectParent(otherKitchenObjectParent);
      }
   }

   [ServerRpc(RequireOwnership = false)]
   private void SwapIngredientsServerRpc(NetworkObjectReference otherKitchenObjectReference) => SwapIngredientsClientRpc(otherKitchenObjectReference);

   [ClientRpc]
   private void SwapIngredientsClientRpc(NetworkObjectReference otherKitchenObjectReference)
   {
      if (!otherKitchenObjectReference.TryGet(out var koNetworkObject)) return;
        
      if (koNetworkObject.TryGetComponent(out IKitchenObjectParent playerKoObjectParentData))
      {
         SwapKitchenObjectParents(playerKoObjectParentData);
      }
   }

   public void SwapKitchenObjectParents(IKitchenObjectParent playerKoObjectParentData)
   {
      KitchenObject.SetTargetTransform(playerKoObjectParentData.DropPoint);
      playerKoObjectParentData.KitchenObject.SetTargetTransform(DropPoint);
      
      (KitchenObject.KitchenObjectParent, playerKoObjectParentData.KitchenObject.KitchenObjectParent) = 
         (playerKoObjectParentData.KitchenObject.KitchenObjectParent, KitchenObject.KitchenObjectParent);
        
      (KitchenObject, playerKoObjectParentData.KitchenObject) = (playerKoObjectParentData.KitchenObject, KitchenObject);
   }
}