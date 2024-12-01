using MSCLoader;
using UnityEngine;
using System.Collections.Generic;

namespace KeySystemExtended
{
    public class KSESoundActions : MonoBehaviour
    {
        public AudioSource carAudioSource;
        public AudioSource playerAudioSource;
        private ModAudio carModAudio = new ModAudio();
        private ModAudio playerModAudio = new ModAudio();

        private Dictionary<string, string> soundPaths;

        // Перечисление категорий для предотвращения ошибок
        public enum SoundCategory
        {
            Player,
            Car
        }

        void Start()
        {
            ConfigureAudioSource(carAudioSource);
            ConfigureAudioSource(playerAudioSource);

            carModAudio.audioSource = carAudioSource;
            playerModAudio.audioSource = playerAudioSource;

            InitializeSoundPaths();
        }

        private void ConfigureAudioSource(AudioSource source)
        {
            source.volume = 0.25f;
            source.loop = false;
            source.clip = null;

            source.spatialBlend = 1f;
            source.minDistance = 1f;
            source.maxDistance = 10f;
            source.rolloffMode = AudioRolloffMode.Linear;
        }

        private void InitializeSoundPaths()
        {
            soundPaths = new Dictionary<string, string>
            {
                { "Player_EngineStarting", "Mods/Assets/KeySystemExtended/Sound/Player/EngineStarting.wav" },
                { "Player_EngineStarted",  "Mods/Assets/KeySystemExtended/Sound/Player/EngineStarted.wav" },
                { "Player_Ping",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping.wav" },
                { "Player_Ping2",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping2.wav" },
                { "Player_Ping3",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping3.wav" },
                { "Car_LockUnlock",        "Mods/Assets/KeySystemExtended/Sound/Car/LockUnlock.wav" }
            };
        }

        public void Play(SoundCategory category, string soundName)
        {
            string key = $"{category}_{soundName}";
            if (soundPaths.TryGetValue(key, out string path))
            {
                ModAudio modAudio = category == SoundCategory.Player ? playerModAudio : carModAudio;
                modAudio.LoadAudioFromFile(path, true, true);
                modAudio.Play();
            }
            else
            {
                ModConsole.Print($"[KSE] Sound not found: {key}");
            }
        }
    }
}
