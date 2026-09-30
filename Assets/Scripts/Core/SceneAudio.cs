using UnityEngine;
using System.Collections.Generic;
using System.Collections;


/// <summary>
/// Play, crossfade, scene audios instructed by Timeline signals or Player's Movement.
/// Does not handle Interactables audio.
/// </summary>
public class SceneAudio : MonoBehaviour
{
    /// <summary>
    /// Informations for the Audio to be played.
    /// </summary>
    [System.Serializable]
    public class AudioConfiguration
    {
        public AudioSource AudioSource;
        public AudioClip AudioClip;
        public float CrossFadeDuration;
        public bool AudioLoop;
    }

    // Inspector
    [Header("Player's Movement Audio")]
    [SerializeField] private CharacterController _characterController;
    [Tooltip("List of all audio configurations aside from timeline audio tracks and interactable audio.")]
    [SerializeField] private List<AudioConfiguration> _audioConfigurations;

    // Fields
    private AudioClip _newAudioClip;
    private AudioClip _currentAudioClip;
    private AudioSource _currentAudioSource;
    private AudioSource _newAudioSource;
    private float _newCrossFadeDuration;
    private bool _isAudioLooping = false;
    private AudioSource _movementAudioSource;
    private AudioClip _movementAudioClip;
    private bool _isUpdatingMovementAudio = false;

    // Unity Lifecycle
    void Update()
    {
        // Keep playing Audio associated with Player's movement.
        if (_isUpdatingMovementAudio)
        {
            UpdateMovementAudioClip(_movementAudioSource, _movementAudioClip);
        }
    }

    // Methods
    /// <summary>
    /// Get the Audio Clip to play from the timeline signal.
    /// </summary>
    public void GetPlayAudioClip(AudioClip audioClip)
    {
        GetAudioConfig(audioClip);
        PlayAudioClip(_newAudioSource, _newAudioClip);
    }

    /// <summary>
    /// Get the Audio Clip to CrossFade from the timeline signal.
    /// </summary>
    public void GetCrossFadeAudioClip(AudioClip audioClip)
    {
        GetAudioConfig(audioClip);
        StartCoroutine(CrossFadeAudioClip(_currentAudioSource, _newAudioSource, _newAudioClip, _newCrossFadeDuration));
    }


    /// <summary>
    /// Get the Audio Clip to play based on Player's movement.
    /// </summary>
    public void GetMovementAudioClip(AudioClip audioClip)
    {
        GetAudioConfig(audioClip);
        _movementAudioSource = _newAudioSource;
        _movementAudioClip = _newAudioClip;
        _isUpdatingMovementAudio = true;
    }

    /// <summary>
    /// Stop playing the Audio once the Player's movement has stopped.
    /// </summary>
    public void StopMovementAudioClip(AudioClip audioClip)
    {
        GetAudioConfig(audioClip);
        _movementAudioSource = _newAudioSource;
        _movementAudioSource.Stop();
        _isUpdatingMovementAudio = false;
    }

    /// <summary>
    /// Set the audio configuration based on the Audio clip received by timeline signal.
    /// </summary>
    private void GetAudioConfig(AudioClip audioClip)
    {
        foreach(AudioConfiguration config in _audioConfigurations)
        {
            if (config.AudioClip == audioClip)
            {
                _newAudioSource = config.AudioSource;
                _newAudioClip = config.AudioClip;
                _newCrossFadeDuration = config.CrossFadeDuration;
                _isAudioLooping = config.AudioLoop;
            }
        }
    }

    /// <summary>
    /// Play Audio Clip on corresponding Audio Source.
    /// </summary>
    private void PlayAudioClip(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.loop = _isAudioLooping;
        audioSource.Stop();
        audioSource.Play();
        _currentAudioClip = audioClip;
        _currentAudioSource = audioSource;
    }

    /// <summary>
    /// Coroutine to crossfade two audio clips playing.
    /// Reduce volume of first (previous) clip while increasing volume of second (next) clip.
    /// </summary>
    /// <param name="previousAudioSource">First audio source.</param>
    /// <param name="nextAudioSource">Second audio source.</param>
    /// <param name="nextAudioClip">Second audio clip.</param>
    /// <param name="crossFadeDuration">Crossfade duration in seconds.</param>
    IEnumerator CrossFadeAudioClip(AudioSource previousAudioSource, AudioSource nextAudioSource, AudioClip nextAudioClip, float crossFadeDuration)
    {
        nextAudioSource.clip = nextAudioClip;
        nextAudioSource.loop = _isAudioLooping;
        // Start second Audio at volume 0.
        nextAudioSource.volume = 0f;
        nextAudioSource.Stop();
        nextAudioSource.Play();
        float time = 0f;
        while (time < crossFadeDuration)
        {
            time += Time.deltaTime; // Track time since crossfade started.
            float t = time / crossFadeDuration; // Calculate crossfade progress from 0 to 1.
            // Gradually fade out previous clip while fading in next clip.
            previousAudioSource.volume = Mathf.Lerp(1f, 0f, t);
            nextAudioSource.volume = Mathf.Lerp(0f, 1f, t);
            // Wait until next frame before continuing crossfade to avoid instant fade.
            yield return null;
        }
        // Stop previous Audio from playing (even at volume 0).
        previousAudioSource.Stop();
        // Reset both Audio Sources to full volume for future crossfades.
        previousAudioSource.volume = 1f;
        nextAudioSource.volume = 1f;
        // Set new current Audio Config to second Audio.
        _currentAudioSource = nextAudioSource;
        _currentAudioClip = nextAudioClip;
    }

    /// <summary>
    /// Update Player's movement audio based on its horizontal movement.
    /// </summary>
    /// <param name="movementAudioSource">Player Audio Source.</param>
    /// <param name="movementAudioClip">Player movement audio clip.</param>
    private void UpdateMovementAudioClip(AudioSource movementAudioSource, AudioClip movementAudioClip)
    {
        movementAudioSource.clip = movementAudioClip;
        Vector3 velocity = _characterController.velocity;
        velocity.y = 0f; // Ignore vertical movement.

        // Check if Player is moving horizontally.
        if (velocity.magnitude > 0.1f)
        {
            // Start playing movement audio if not already playing.
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
        }
        // Stop playing movement audio if Player is no longer moving.
        else
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
        }
    }
}
