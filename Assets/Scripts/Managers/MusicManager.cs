using UnityEngine;

[DefaultExecutionOrder(-400)]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [SerializeField] private AudioSource _musicSource;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _musicSource = GetComponent<AudioSource>();
    }
#endif

    private void Awake()
    {
        Instance = this;
    }

    public float MusicLevel
    {
        get
        {
            var val = PlayerPrefs.GetFloat("MusicLevel", 1);
            _musicSource.volume = val;
            return val;
        }
        set
        {
            _musicSource.volume = value;
            PlayerPrefs.SetFloat("MusicLevel", value);
        }
    }
}
