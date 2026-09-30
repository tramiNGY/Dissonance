using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// Play Interactable Audio each time corresponding interaction is detected.
/// </summary>
public class InteractionAudio : MonoBehaviour
{
    [System.Serializable]
    public class InteractionAudioConfiguration
    {
        public string Interaction;
        public AudioClip AudioClip;
    }

    // Inspector
    [SerializeField] private AudioSource _audioSource;
    [Tooltip("Interactable Audio for Element 0: Press, Element 1: Grab, Element 2: Zone")]
    [SerializeField] private List<InteractionAudioConfiguration> _interactionAudioConfigurations;
    
    // Methods
    public void PressAudio(SelectEnterEventArgs args)
    {
        _audioSource.PlayOneShot(_interactionAudioConfigurations[0].AudioClip);
    }

    public void GrabAudio(SelectEnterEventArgs args)
    {
        _audioSource.PlayOneShot(_interactionAudioConfigurations[1].AudioClip);
    }

    public void ZoneAudio()
    {
        _audioSource.PlayOneShot(_interactionAudioConfigurations[2].AudioClip);
    }
}
