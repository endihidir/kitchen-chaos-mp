using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer _headMeshRenderer, _bodyMeshRenderer;

    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        
        _headMeshRenderer.GetPropertyBlock(_materialPropertyBlock);
    }

    public void SetPlayerColor(Color color)
    {
        _materialPropertyBlock.SetColor("_BaseColor", color);
        _headMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        _bodyMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}