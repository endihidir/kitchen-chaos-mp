using UnityEngine;

public class CharacterColorSelectUI : MonoBehaviour
{
    [SerializeField] private ColorButtonUI[] _colorButtonUI;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _colorButtonUI = GetComponentsInChildren<ColorButtonUI>(true);

        for (int i = 0; i < _colorButtonUI.Length; i++)
        {
            _colorButtonUI[i].ColorId = i;
        }
    }
#endif

}