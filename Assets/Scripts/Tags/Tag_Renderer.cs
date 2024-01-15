using UnityEngine;

public class Tag_Renderer : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    public MeshRenderer MeshRenderer => _meshRenderer;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
#endif
}
