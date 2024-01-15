using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Sound")]
public class SoundSO : ScriptableObject
{
    public SoundType soundType;
    public AudioClip[] audioClips;
}
