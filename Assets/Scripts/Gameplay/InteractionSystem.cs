using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


/// <summary>
/// Link Timeline signals to Player's interactions.
/// Pause Timeline while waiting for specified interaction.
/// Validate interaction received.
/// Resume playing Timeline upon successful interaction or timeout reached.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    /// <summary>
    /// Interaction's informations regarding timeline for an interactable object.
    /// </summary>
    [System.Serializable]
    public class InteractionConfiguration
    {
        [Tooltip("ID of the Interaction awaited by timeline.")]
        public string WaitInteraction;
        [Tooltip("ID of the successful interaction performed.")]
        public string ExpectedInteraction;
        [Tooltip("Ordered list of interactions to perform to succeed complete interaction.")]
        public List<string> ExpectedInteractionSequence;
        [Tooltip("Time delay before resuming timeline upon player's fail attempt to perform the interaction awaited.")]
        public float TimeoutInteraction;
    }

    // Inspector
    [Header("Playable Director")]
    [Tooltip("Main Scene Timeline")]
    [SerializeField] private PlayableDirector _director;
    [Tooltip("Alternative Scene Timeline")]
    [SerializeField] private PlayableDirector _alternativeDirector;
    [SerializeField] private List<InteractionConfiguration> _interactionConfigurations;
    
    // Fields
    private string _currentWaitInteraction = null; // Current interaction's ID awaited at one point of the timeline.
    private string _currentReceivedInteraction; // Current interaction performed by the Player.
    private InteractionConfiguration _currentInteractionConfig;
    private int _currentInteractionSequenceIndex;

    // Unity Lifecycle
    void Awake()
    {
        // Prevent Raycast Select far objects to only detect interactables at near distance for realism purpose.
        var interactors = FindObjectsByType<NearFarInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var interactor in interactors)
        {
            interactor.enableFarCasting = false;
        }
    }
    
    // Methods
    /// <summary>
    /// Set the current interaction ID awaited from the timeline.
    /// </summary>
    /// <param name="waitInteraction">ID of the interaction awaited by the timeline.</param>
    public void SetWaitInteraction(string waitInteraction)
    {
        _currentWaitInteraction = waitInteraction;
        Debug.Log("Current Wait interaction is " + _currentWaitInteraction);
        GetInteractionConfig(_currentWaitInteraction);
        _director.Pause();
        Debug.Log("Timeline has paused");
        StartCoroutine(InteractionTimeoutCountdown());
    }

    /// <summary>
    /// Set the current interaction received ID from InteractionHandler each time a new interaction is performed by the Player.
    /// </summary>
    /// <param name="interactionReceived">ID of the interaction received from the interactable.</param>
    public void ReceiveInteraction(string interactionReceived)
    {
        Debug.Log("Receive at :" + Time.time + " : " + interactionReceived);
        // If no interaction is awaited by timeline, ignore the received interaction.
        if (_currentWaitInteraction == null)
        {
            return;
        }
        _currentReceivedInteraction = interactionReceived;
        Debug.Log("Current interaction received: " + _currentReceivedInteraction);
        CompareInteraction();
    }

    /// <summary>
    /// Find the current awaited ID in the list of interactions configs and set the current interaction awaited configuration.
    /// </summary>
    /// <param name="currentWaitInteraction">ID of Current interaction awaited.</param>
    public void GetInteractionConfig(string currentWaitInteraction)
    {
        foreach(InteractionConfiguration config in _interactionConfigurations)
        {
            if (config.WaitInteraction == currentWaitInteraction)
            {
                _currentInteractionConfig = config;
                Debug.Log("Current Interaction config is: " + "\n" 
                + "WaitInteraction: " + _currentInteractionConfig.WaitInteraction + "\n" 
                + "ExpectedInteraction: " + _currentInteractionConfig.ExpectedInteraction + "\n" 
                + "ExpectedInteractionSequence: " + _currentInteractionConfig.ExpectedInteractionSequence + "\n"
                + "InteractionTimeout: " + _currentInteractionConfig.TimeoutInteraction);
            }
        }
    }

    /// <summary>
    /// Compare and check if the interaction received correspond to the expected interaction associated with the current awaited interaction.
    /// </summary>
    public void CompareInteraction()
    {
        if (_currentWaitInteraction == null)
        {
            return;
        }
        // Single interaction
        if (_currentInteractionConfig.ExpectedInteractionSequence == null || _currentInteractionConfig.ExpectedInteractionSequence.Count == 0)
        {
            // Received Interaction correspond to Interaction Awaited => Continue Timeline.
            if (_currentReceivedInteraction == _currentInteractionConfig.ExpectedInteraction)
            {
                Debug.Log($"Interaction {_currentReceivedInteraction} has succeeded");
                ContinueTimeline();
            }
            // Alternative Interaction: special type of interaction if the Player has multiple interaction's choices at one point of the timeline.
            // Play the Alternative Timeline upon alternative interaction detected.
            else if (_currentReceivedInteraction == "AlternativeInteraction") // "AlternativeInteraction" ID defining all alternative interactables.
            {
                if (_alternativeDirector != null)
                {
                    _alternativeDirector.Play();
                    Debug.Log("Alternative Timeline has started playing");
                }
            }
            // Received Interaction does not correspond to Interaction Awaited.
            return;
        }

        // Sequence of interactions
        string currentSequenceInteraction = _currentInteractionConfig.ExpectedInteractionSequence[_currentInteractionSequenceIndex];
        // One interaction of the sequence has succeeded in the correct sequence's order.
        if (_currentReceivedInteraction == currentSequenceInteraction)
        {
            Debug.Log($"Sequence step at Index {_currentInteractionSequenceIndex} of Interaction {currentSequenceInteraction} has succeeded");
            _currentInteractionSequenceIndex++; // Increment to next interaction of the sequence.
            // Complete Sequence of interactions Performed => Continue Timeline.
            if (_currentInteractionSequenceIndex >= _currentInteractionConfig.ExpectedInteractionSequence.Count) // Total number of interactions in the sequence reached.
            {
                Debug.Log($"Interaction sequence {_currentInteractionConfig.ExpectedInteraction} has succeeded");
                ContinueTimeline();
            }      
        }

        // Reset the progress of validation of the sequence if an interaction is incorrect or misordered.
        else
        {
            Debug.Log("Interaction does not correspond to expected interaction in sequence");
            _currentInteractionSequenceIndex = 0;
        }
    }

    /// <summary>
    /// Reset awaited interaction then continue to play the timeline at the point it has stopped when awaiting the previous interaction.
    /// </summary>
    public void ContinueTimeline()
    {
        _currentWaitInteraction = null;
        _currentInteractionSequenceIndex = 0;
        _director.Play();
        Debug.Log("Timeline has resumed play");
    }

    /// <summary>
    /// Coroutine starting at received awaited Interaction from timeline and wait for Interaction TimeOut reached.
    /// Permit to resume playing timeline after a certain amount of time to avoid Player being blocked if interactions fail or take too much time.
    /// </summary>
    IEnumerator InteractionTimeoutCountdown()
    {
        float timeout = _currentInteractionConfig.TimeoutInteraction;
        string currentWait = _currentInteractionConfig.WaitInteraction;
        yield return new WaitForSeconds(timeout);
        Debug.Log("Interaction Timeout reached");
        // After Timeout reached, if current awaited interaction is still the same as when Timeout countdown started, resume playing.
        if (_currentWaitInteraction == currentWait)
        {
            ContinueTimeline();
        }
        // If current wait Interaction is different: Player has succeeded interaction within timeout and timeline has already resumed playing and encoutered next awaited interaction.
    }
}
