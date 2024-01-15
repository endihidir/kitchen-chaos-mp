using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-400)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundListSO _soundListSo;

    [SerializeField] private AudioSourceController _audioSourcePrefab;

    [SerializeField] private List<AudioSourceController> _loopingAudioSourceControllers = new List<AudioSourceController>();

    private Camera _cam;

    public float SoundLevel
    {
        get => PlayerPrefs.GetFloat("SoundLevel", 1);
        set => PlayerPrefs.SetFloat("SoundLevel", value);
    }

    private void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    public void PlaySound(SoundType soundType, float volume, bool random = true) => CreateSound(soundType, volume, _cam.transform.position, random);
    public void PlaySoundAtPosition(SoundType soundType, float volume, Vector3 position, bool random = true) => CreateSound(soundType, volume, position, random);
    
    private void CreateSound(SoundType soundType, float volume, Vector3 position, bool random = true)
    {
        var audioClip = SelectAudioClip(soundType, random);
        if(!audioClip) return;
        AudioSource.PlayClipAtPoint(audioClip, position, volume * SoundLevel);
    }

    public void PlayLoopSoundAtPosition(SoundType soundType, float volume, float stayDuration)
    {
        var audioClip = SelectAudioClip(soundType);
        if(!audioClip) return;

        var audioSourceController = Instantiate(_audioSourcePrefab, _cam.transform.position, Quaternion.identity);
        audioSourceController.name = soundType.ToString();
        audioSourceController.audioSourceControllers = _loopingAudioSourceControllers;
        audioSourceController.soundType = soundType;
        audioSourceController.DestroyWait(stayDuration);
        _loopingAudioSourceControllers.Add(audioSourceController);

        var audioSource = audioSourceController.AudioSource;
        audioSource.clip = audioClip;
        audioSource.volume = volume * SoundLevel;
        audioSource.Play();
    }

    public void RemoveLoopedSound(SoundType soundType)
    {
        var audioSourceController = _loopingAudioSourceControllers.FirstOrDefault(x => x.soundType.Equals(soundType));
        if (!audioSourceController) return;
        audioSourceController.Destroy();
    }

    private AudioClip SelectAudioClip(SoundType soundType, bool random = true)
    {
        var audioClips = _soundListSo.soundDataList.FirstOrDefault(x => x.soundType.Equals(soundType))?.audioClips;
        if (audioClips is null) return null;
        if (audioClips.Length < 1) return null;
        var index = random ? Random.Range(0, audioClips.Length) : 0;
        var randomClip = audioClips[index];
        return randomClip;
    }
}