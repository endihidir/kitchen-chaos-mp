public class DeliveryCounter : BaseCounter
{
    public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
    {
        if (otherKitchenObjectParent.HasKitchenObject)
        {
            if (otherKitchenObjectParent.KitchenObject is PlateKitchenObject plateKitchenObject)
            {
                var isDeliverySuccessful = DeliveryManager.Instance.TryDeliverRecipe(plateKitchenObject);

                if (isDeliverySuccessful)
                {
                    //Debug.LogAssertion("Player delivered the correct recipe!");
                    
                    otherKitchenObjectParent.KitchenObject.DestroySelf();
                    
                    InvokeStart();
                }
                else
                {
                    InvokeInterrupt();
                    
                    //Debug.LogError("Delivery not matched with plate!");
                }
            }
        }
    }
}