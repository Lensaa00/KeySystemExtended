using MSCLoader;
using UnityEngine;
using System.Collections.Generic;

namespace KeySystemExtended
{
    public class KSESoundActions : MonoBehaviour
    {
        private AudioSource audioSource;
        private ModAudio modAudio = new ModAudio();

        private Dictionary<string, string> soundPaths = new Dictionary<string, string>();

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.25f;
            audioSource.loop = false;
            audioSource.clip = null;

            audioSource.spatialBlend = 1f; // Полностью 3D-звук
            audioSource.minDistance = 1f; // Минимальная дистанция, на которой звук воспроизводится на полной громкости
            audioSource.maxDistance = 10f; // Максимальная дистанция, на которой звук затихает
            audioSource.rolloffMode = AudioRolloffMode.Linear; // Линейное затухание громкости

            modAudio.audioSource = audioSource;

            // Инициализация путей к звукам
            soundPaths = new Dictionary<string, string>
            {
                { "player_EngineStarting", "Mods/Assets/KeySystemExtended/Sound/Player/EngineStarting.wav" },
                { "player_EngineStarted",  "Mods/Assets/KeySystemExtended/Sound/Player/EngineStarted.wav" },
                { "player_Ping",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping.wav" },
                { "player_Ping2",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping2.wav" },
                { "player_Ping3",  "Mods/Assets/KeySystemExtended/Sound/Player/Ping3.wav" },
                { "car_LockUnlock",        "Mods/Assets/KeySystemExtended/Sound/Car/LockUnlock.wav" }
            };
        }

        public void Play(string category, string soundName)
        {
            string key = $"{category}_{soundName}";
            if (soundPaths.TryGetValue(key, out string path))
            {
                modAudio.LoadAudioFromFile(path, true, true);
                modAudio.Play();
                ModConsole.Print($"[KSE] Played sound on {gameObject.name}: {path}");
            }
            else
            {
                ModConsole.Print($"[KSE] Sound not found: {key}");
            }
        }
    }
}
