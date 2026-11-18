using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

namespace Dino.Utility.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioManagerData _audioManagerData;
        public string _soundNameTest;
        private readonly Dictionary<string, AudioSource> sources = new();
        private AudioSource music;
        private float musicEnergy = 0.5f;
        private float musicFullVolume = 1f;

        public AudioManagerData AudioManagerData
        {
            get => _audioManagerData;
            set => _audioManagerData = value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            foreach (AudioData sound in _audioManagerData.audioData)
            {
                if (sound.clip == null || sources.ContainsKey(sound.name))
                    continue;

                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = MixerGroup(sound.audioType);
                source.clip = sound.clip;
                source.volume = sound.volume;
                source.pitch = sound.pitch;
                source.loop = sound.loop;
                sources.Add(sound.name, source);
            }

            float volume = PlayerPrefs.GetFloat("GameVolume", 0.5f);
            UpdateAudioMixerGroupVolume(PlayerPrefs.GetInt("MusicEnabled", 1) == 1 ? volume : 0f, AudioType.Music);
            UpdateAudioMixerGroupVolume(PlayerPrefs.GetInt("SFXEnabled", 1) == 1 ? volume : 0f, AudioType.SFX);
            if (sources.TryGetValue("backgroundMusic", out music))
            {
                musicFullVolume = music.volume;
                music.volume *= musicEnergy;
                music.Play();
            }
        }

        private void Update()
        {
            if (music != null)
                music.volume = Mathf.MoveTowards(music.volume, musicFullVolume * musicEnergy, Time.unscaledDeltaTime * 1.8f);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetMusicEnergy(float target) => musicEnergy = Mathf.Clamp01(target);

        public void PlaySound(string soundName)
        {
            if (sources.TryGetValue(soundName, out AudioSource source))
                source.Play();
        }

        public void StopSound(string soundName)
        {
            if (sources.TryGetValue(soundName, out AudioSource source))
                source.Stop();
        }

        public void UpdateAudioMixerGroupVolume(float value, AudioType audioType)
        {
            AudioMixerGroup group = MixerGroup(audioType);
            if (group == null)
                return;
            float volume = Mathf.Clamp01(value);
            float decibels = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f;
            string parameter = audioType switch
            {
                AudioType.Music => _audioManagerData.MusicVolumeParameterName,
                AudioType.Ambience => _audioManagerData.AmbienceVolumeParameterName,
                _ => _audioManagerData.SfxVolumeParameterName
            };
            group.audioMixer.SetFloat(parameter, decibels);
        }

        private AudioMixerGroup MixerGroup(AudioType audioType) => audioType switch
        {
            AudioType.Music => _audioManagerData.MusicAudioMixerGroup,
            AudioType.Ambience => _audioManagerData.AmbienceAudioMixerGroup,
            _ => _audioManagerData.SfxAudioMixerGroup
        };

        [Button]
        private void TestSound() => PlaySound(_soundNameTest);
    }

    public enum AudioType { Music, SFX, Ambience }
}
