public class TrashCounter : BaseCounter
{
    public override void Interact(IKitchenObjectParent otherKitchenObjectParent)
    {
        if (otherKitchenObjectParent.HasKitchenObject)
        {
            SoundManager.Instance.PlaySound(SoundType.Trash, 0.35f);
            
            otherKitchenObjectParent.KitchenObject.DestroySelf();
        }
    }
}