using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Slider _soundSlider, _musicSlider;

    [SerializeField] private GameObject _optionsPanel;

    private void OnEnable()
    {
        _soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void OnDisable()
    {
        _soundSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);
        _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }

    private void Awake()
    {
        _soundSlider.value = SoundManager.Instance.SoundLevel;
        _musicSlider.value = MusicManager.Instance.MusicLevel;
        
        SetActivePanel(false);
    }

    private void OnSoundVolumeChanged(float volume)
    {
        SoundManager.Instance.SoundLevel = volume;
    }

    private void OnMusicVolumeChanged(float volume)
    {
        MusicManager.Instance.MusicLevel = volume;
    }

    public void SetActivePanel(bool active)
    {
        _optionsPanel.SetActive(active);

        if (active)
        {
            _soundSlider.Select();
        }
    }
}