
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Dino.Utility.Audio;
using AudioType = Dino.Utility.Audio.AudioType;

public class SettingsUI : UIWindow
{
    [Header("SettingsUI")]
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;

    public override void Initialize()
    {
        base.Initialize();

        float volume =
            PlayerPrefs.GetFloat("GameVolume", 0.5f);

        bool musicEnabled =
            PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        bool sfxEnabled =
            PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        // Asignar valores sin activar eventos.
        _volumeSlider.SetValueWithoutNotify(volume);
        _musicToggle.SetIsOnWithoutNotify(musicEnabled);
        _sfxToggle.SetIsOnWithoutNotify(sfxEnabled);

        // Escuchar cambios de los controles.
        _volumeSlider.onValueChanged.AddListener(
            OnVolumeChanged);

        _musicToggle.onValueChanged.AddListener(
            OnMusicChanged);

        _sfxToggle.onValueChanged.AddListener(
            OnSFXChanged);

        ApplyVolume();
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();

        ApplyVolume();
    }

    private void OnMusicChanged(bool enabled)
    {
        PlayerPrefs.SetInt(
            "MusicEnabled", enabled ? 1 : 0);

        PlayerPrefs.Save();

        ApplyVolume();
    }

    private void OnSFXChanged(bool enabled)
    {
        PlayerPrefs.SetInt(
            "SFXEnabled", enabled ? 1 : 0);

        PlayerPrefs.Save();

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (AudioManager.Instance == null)
            return;

        float volume = _volumeSlider.value;

        float musicVolume =
            _musicToggle.isOn ? volume : 0f;

        float sfxVolume =
            _sfxToggle.isOn ? volume : 0f;

        AudioManager.Instance.UpdateAudioMixerGroupVolume(
            musicVolume, AudioType.Music);

        AudioManager.Instance.UpdateAudioMixerGroupVolume(
            sfxVolume, AudioType.SFX);
    }

    public void Back()
    {
        UIManager.Instance.HideWindow("settingsui");
    }

    private void OnDestroy()
    {
        _volumeSlider.onValueChanged.RemoveListener(
            OnVolumeChanged);

        _musicToggle.onValueChanged.RemoveListener(
            OnMusicChanged);

        _sfxToggle.onValueChanged.RemoveListener(
            OnSFXChanged);
    }

    [Button("Test Show")]
    private void TestShow()
    {
        Show();
    }

    [Button("Test Hide")]
    private void TestHide()
    {
        Hide();
    }
}

