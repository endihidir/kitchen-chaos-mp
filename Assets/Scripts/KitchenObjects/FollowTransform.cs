using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform _targetTransform;

    private Vector3 _positionOffset;
    public void InitializeTarget(Transform targetTransform, Vector3 offset)
    {
        _targetTransform = targetTransform;
        _positionOffset = offset;
    }

    private void LateUpdate()
    {
        if(!_targetTransform) return;

        transform.position = _targetTransform.position + _positionOffset;
        
        transform.rotation = _targetTransform.rotation;
    }
}