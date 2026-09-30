using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// Activate and play specified timeline at setup start time.
/// </summary>
public class TimelineController : MonoBehaviour
{
    // Inspector
    [Tooltip("All timelines in the scene.")]
    [SerializeField] private List<GameObject> _timelines;

    // Fields
    private PlayableDirector _director;

    // Unity Lifecycle
    void Start()
    {
        if (ScenesManager.Instance == null)
        {
            Debug.LogError("TimelineController: ScenesManager.instance is null");
        }
        // Retrieve timeline ID setup from ScenesManager at previous scene.
        int timelineID = ScenesManager.Instance.GetNextTimelineID();
        // Scene only contains one timeline.
        if (timelineID == -1)
        {
            timelineID = 0;
        }
        // Retrieve timeline start time setup from ScenesManager at previous scene.
        float startTime = ScenesManager.Instance.GetNextTimelineStartTime();
        ActivateTimeline(timelineID, startTime);
        ScenesManager.Instance.ResetNextTimelineID();
    }

    // Methods
    /// <summary>
    /// Activate and play a Timeline by its ID. Notice: All timelines are on "Play on Awake" unchecked.
    /// </summary>
    private void ActivateTimeline(int timelineID, float startTime = 0f)
    {
        // Set all timelines inactive in case some timelines were manually activated for tests.
        foreach(GameObject timeline in _timelines)
        {
            timeline.SetActive(false);
        }
        _timelines[timelineID].SetActive(true);
        _director = _timelines[timelineID].GetComponent<PlayableDirector>();
        _director.time = startTime; // Place Timeline cursor at specified start time.
        _director.Evaluate(); // Evaluate PlayableGraph at specified start time to apply timeline state at that point.
        _director.Play();
    }
}
