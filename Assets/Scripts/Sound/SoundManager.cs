using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource trackSource;
    public bool IsMuted { get; private set; }

    public void ToggleMute() {
        IsMuted = !IsMuted;
        audioSource.volume = IsMuted ? 0f : 1f;
        trackSource.volume = IsMuted ? 0f : 1f;
    }

    #region Singleton
    private static SoundManager _instance;

    public static SoundManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        _instance.IsMuted = false;
    }
    #endregion

    public void PlayAudioClip(AudioClip audioClip, AudioClipOptions audioOptions) {
        audioSource.clip = audioClip;
        audioSource.loop = audioOptions.loop;
        audioSource.Play();
    }

    public void StopAudioClip()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    public void PlayBackground(AudioClip audioClip, AudioClipOptions audioOptions, bool interruptAudio)
    {
        if (!interruptAudio && trackSource.isPlaying)
            return;
        trackSource.clip = audioClip;
        trackSource.loop = audioOptions.loop;
        trackSource.Play();
    }
    public void StopBackground()
    {
        if (trackSource.isPlaying)
            trackSource.Stop();
    }
}
