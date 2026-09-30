using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;


/// <summary>
/// Handle all possible interactions by the Player with the configured interactable.
/// Listen to XR Interaction Toolkit gesture detection.
/// Send performed interaction to InteractionSystem as received interaction.
/// </summary>
public class InteractionHandler : MonoBehaviour
{
    /// <summary>
    /// List all interactions's IDs detected per type for each interactable.
    /// Each ID correspond to the received interaction sent to InteractionSystem.
    /// </summary>
    [System.Serializable]
    public class InteractionTypes
    {
        [Tooltip("Interaction's ID when interactable has been grabbed.")]
        public string Grab;
        [Tooltip("Interaction's ID when interactable has stopped being grabbed.")]
        public string Release;
        [Tooltip("Interaction's ID when interactable has been poked.")]
        public string Press;
        [Tooltip("Interaction's ID when interactable has entered triggered zone.")]
        public string Zone;
        [Tooltip("Tag of the gameObject defining the Trigger Zone.")]
        public string ZoneTag;
        [Tooltip("Interaction's ID when Player's camera is looking at interactable.")]
        public string LookAt;
        [Tooltip("Interaction's ID when Player hover over interactable.")]
        public string Hover;
        [Tooltip("Minimum duration in seconds for Player to maintain when hovering over interactable.")]
        public float DurationTime;
    }

    // Inspector
    [Header("Interaction Event")]
    [SerializeField] private UnityEvent<string> _OnInteractionDetected;
    [Header("Interactions detected")]
    [Tooltip("Interaction detected ID.")]
    [SerializeField] private InteractionTypes _interaction;
    [Header("Zone Interaction Configuration")]
    [Tooltip("Interaction Audio script of interactable.")]
    [SerializeField] private InteractionAudio _interactionZoneAudio;
    [Header("LookAt Interaction Configuration")]
    [Tooltip("Interactor Player's Main Camera.")]
    [SerializeField] private Transform _cameraTransform;
    [Tooltip("Maximum angle between Camera's forward direction towards interactable to be considered in Camera's view.")]
    [SerializeField] float _lookAtLimitAngle;

    // Fields
    private Vector3 _lookAtDirection;
    private float _lookAtAlignment;
    private bool _isLookingAt = false;

    private bool _isInZone = false;
    private bool _isHovering = false;
    private Coroutine _hoverCoroutine;

    // Unity Lifecycle
    void Update()
    {
        // Check if Player is looking at interactable while inside Zone.
        if (_isInZone && !string.IsNullOrEmpty(_interaction.LookAt))
        {
            InteractableLookAt();
        }
    }

    /// <summary>
    /// Interaction Enter trigger zone detected when Player's or other interactable's collider is in specific area.
    /// </summary>
    /// <param name="other">Player's or other interactable's collider.</param>
    void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(_interaction.ZoneTag))
        {
            return;
        }
        if (other.CompareTag(_interaction.ZoneTag))
        {
            Debug.Log("Interaction OnTriggerEnter detected " + _interaction.Zone);
            _isInZone = true;
            // Play Interaction zone Audio.
            if (_interactionZoneAudio != null)
            {
                _interactionZoneAudio.ZoneAudio();
            }
            // Notify subscribed listeners by passing detected Zone Interaction ID.
            _OnInteractionDetected.Invoke(_interaction.Zone);
        }
    }

    /// <summary>
    /// Interaction Exit trigger zone detected when Player's or other interactable's collider is no longer in specific area.
    /// </summary>
    /// <param name="other">Player's or other interactable's collider.</param>
    void OnTriggerExit(Collider other)
    {
        if (string.IsNullOrEmpty(_interaction.ZoneTag))
        {
            return;
        }
        if (other.CompareTag(_interaction.ZoneTag))
        {
            Debug.Log("Interaction OnTriggerExit detected exit zone: " + _interaction.Zone);
            _isInZone = false;
            _isLookingAt = false;
        }
    }

    // Methods

    // XR Interaction Toolkit
    /// <summary>
    /// Called when XR Grab Interactable starts detecting grabbing interaction (Select Entered).
    /// </summary>
    public void InteractableGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Interaction Grab detected " + _interaction.Grab);
        // Notify subscribed listeners by passing detected Grab Interaction ID.
        _OnInteractionDetected.Invoke(_interaction.Grab);
    }

    /// <summary>
    /// Called when XR Simple Interactable starts detecting poking interaction (Select Entered).
    /// </summary>
    public void InteractablePressed(SelectEnterEventArgs args)
    {
        Debug.Log("Interaction Press detected " + _interaction.Press);
        // Notify subscribed listeners by passing detected Press Interaction ID.
        _OnInteractionDetected.Invoke(_interaction.Press);
    }

    /// <summary>
    /// Called when XR Simple Interactable starts detecting hovering interaction (Hover Entered).
    /// </summary>
    public void InteractableHovered(HoverEnterEventArgs args)
    {
        Debug.Log("Interaction Hover detected " + _interaction.Hover);
        // Debug.Log($"Hover Interactor: {args.interactorObject.transform.name}, Hover Interactable: {args.interactableObject.transform.name}");
        _isHovering = true;
        // Start Hover duration countdown if duration is configured.
        if (_interaction.DurationTime != 0)
        {
            _hoverCoroutine = StartCoroutine(InteractionDurationChrono()); // Store reference to coroutine.
        } 
    }

    /// <summary>
    /// Called when XR Simple Interactable stops detecting hovering interaction (Hover Exited).
    /// </summary>
    public void InteractableStopHovered(HoverExitEventArgs args)
    {
        Debug.Log("Interaction Stop Hover detected " + _interaction.Hover);
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null; // Clear coroutine reference (no coroutine currently active).
        }
        _isHovering = false;
    }

    // Spatial
    /// <summary>
    /// Check if Player's camera is looking at interactable within a limited view angle.
    /// </summary>
    private void InteractableLookAt()
    {
        if (_cameraTransform == null)
        {
            Debug.LogError("Missing Camera not assigned in Inspector for LookAt interaction");
            return;
        }
        if (_isInZone)
        {   
            // Calculate the normalized direction from Player's Camera's position to interactable's position.
            _lookAtDirection = (transform.position - _cameraTransform.position).normalized;
            // Calculate the alignment between Player's Camera forward direction and the direction towards the interactable.
            // Dot returns cosinus of angle between two normalized directions.
            // (dot = 1: Aligned in same direction, dot = 0: Perpendicular, dot = -1: Aligned in opposite direction).
            _lookAtAlignment = Vector3.Dot(_cameraTransform.forward, _lookAtDirection);
            // Debug.Log($"LookAtAlignment: {_lookAtAlignment}, LookAtLimitAngle: {_lookAtLimitAngle}, cos(lookAtLimitAngle): {Mathf.Cos(_lookAtLimitAngle * Mathf.Deg2Rad)}");
            
            // Check whether the interactable is within the viewing angle limit.
            if (_lookAtAlignment >= Mathf.Cos(_lookAtLimitAngle * Mathf.Deg2Rad)) // Cosinus value decreases when angle value increases.
            {
                _isLookingAt = true;
                // Debug.Log($"Player is LookingAt: {_isLookingAt}, Interaction LookAt detected {_interaction.LookAt}");
                // Notify subscribed listeners by passing detected LookAt Interaction ID.
                _OnInteractionDetected.Invoke(_interaction.LookAt);
            }
            else
            {
                _isLookingAt = false;
            }
        }
    }

    // Time
    /// <summary>
    /// Coroutine waiting for configured interaction duration before sending Hovering detected event.
    /// Permit to wait a certain amount of time while hovering to make sure Player has not mistakenly hovered interactable without chosing it.
    /// </summary>
    IEnumerator InteractionDurationChrono()
    {
        float duration = _interaction.DurationTime;
        yield return new WaitForSeconds(duration);
        if (_isHovering)
        {
            Debug.Log("Hover duration chrono reached for: " + gameObject.name);
            _OnInteractionDetected.Invoke(_interaction.Hover);
        }
    }
}
