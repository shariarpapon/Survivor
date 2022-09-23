using UnityEngine;
using UnityEngine.SceneManagement;

namespace Survivor.Core
{ 
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip notificationAlert;
        [SerializeField] private AmbientAudio[] ambientAudioClips;


        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        private void OnEnable() 
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() 
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Awake() 
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start() 
        {
            UpdateVolumes();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
        {
            if (scene.name == "MainMenu") SetAmbientMusic(AmbientAudioType.MainMenu);
            else if (scene.name == "Game") SetAmbientMusic(AmbientAudioType.None);
        }

        public void UpdateVolumes() 
        {
            AudioListener.volume = GameManager.Instance.userPreference.masterVolume;
            sfxSource.volume = GameManager.Instance.userPreference.sfxVolume;
            musicSource.volume = GameManager.Instance.userPreference.musicVolume;
            ambientSource.volume = GameManager.Instance.userPreference.ambientVolume;
        }

        public void SetAmbientMusic(AmbientAudioType type) 
        {
            if (type == AmbientAudioType.None)
            {
                ambientSource.clip = null;
                return;
            }

            foreach (AmbientAudio aud in ambientAudioClips)
            {
                if (aud.type == type) ambientSource.clip = aud.clip;
                ambientSource.Play();
            }
        }

        public void PlayButtonClick() 
        {
            sfxSource.PlayOneShot(buttonClick);
        }

        public void PlayButtonHover() 
        {
            sfxSource.PlayOneShot(buttonHover);
        }

        public void PlayNotificationAlert() 
        {
            sfxSource.PlayOneShot(notificationAlert);
        }
    }


}
