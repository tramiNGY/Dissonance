using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Manage Scene change to other scenes.
/// Indicate to TimelineController which timeline and start time to play.
/// Permit to quit app.
/// </summary>
public class ScenesManager : MonoBehaviour
{
    // Instance
    public static ScenesManager Instance {get; private set;} // Global reference accessible from other scripts.

    // Inspector
    /// <summary>
    /// Test and change to specified scene by skipping all scenes between startup scene and targeted scene.
    /// </summary>
    [Header("Skip To Scene")]
    [Tooltip("Check box to activate skipping scene.")]
    [SerializeField] private bool _isSkippingScene = false;
    [Tooltip("Name of the Scene to skip to. To be reset to blank field when not skipping scene.")]
    [SerializeField] private string _skipToSceneName;
    [Tooltip("Timeline ID of the scene to skip to.")]
    [SerializeField] private int _skipToSceneTimelineID = 0;
    [Tooltip("Start Time of the timeline to skip to. Notice: depending on start time, some event signals might not have been activated at the point of start chosen.")]
    [SerializeField] private float _skipToSceneStartTimeline = 0;

    // Fields
    private int _nextTimelineID = -1;
    private float _nextTimelineStartTime = 0f;
    
    // Unity Lifecycle
    void Awake()
    {
        // Singleton: Only One ScenesManager persists across scenes.
        // Hold informations about next scene name, Timeline ID and Start time to be loaded and played.

        // An ScenesManager instance already exist.
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        // No existing instance of ScenesManager => this becomes Singleton instance.
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep ScenesManager alive across scene changes.
        }
    }

    void Start()
    {
        // Skip To Scene is activated.
        if (_isSkippingScene && !string.IsNullOrEmpty(_skipToSceneName))
        {
            SetNextTimelineID(_skipToSceneTimelineID);
            SetNextTimelineStartTime(_skipToSceneStartTimeline);
            ChangeScene(_skipToSceneName);
            _isSkippingScene = false;
        }
    }

    // Methods
    /// <summary>
    /// Asynch Load next scene.
    /// </summary>
    /// <param name="sceneName">Same name as the scene Asset.</param>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Set which timeline to start at the next scene. Default timeline ID 0 for scenes containing only one timeline.
    /// </summary>
    /// <param name="timelineID">Timeline ID in chronological order of the story starting from ID 0.</param>
    public void SetNextTimelineID(int timelineID)
    {
        _nextTimelineID = timelineID;
    }

    /// <summary>
    /// Enable TimelineController to retrieve information from ScenesManager about the next timeline to play.
    /// </summary>
    public int GetNextTimelineID()
    {
        return _nextTimelineID;
    }
    
    /// <summary>
    /// Set at which second the next timeline will start.
    /// Default Beginning of timeline is at 0s.
    /// Gameplay: Alternative timeline or Flashback can start at an advanced time of the timeline.
    /// Debug: Skip to a certain point at timeline to test.
    /// </summary>
    /// <param name="timelineStartTime">Desired Start point of timeline in seconds.</param>
    public void SetNextTimelineStartTime(float timelineStartTime)
    {
        _nextTimelineStartTime = timelineStartTime;
    }

    /// <summary>
    /// Enable TimelineController to retrieve information from ScenesManager about the next timeline start time.
    /// </summary>
    public float GetNextTimelineStartTime()
    {
        return _nextTimelineStartTime;
    }

    /// <summary>
    /// Enable TimelineController to reset next timeline information from ScenesManager after finishing activation.
    /// </summary>
    public void ResetNextTimelineID()
    {
        _nextTimelineID = -1;
        _nextTimelineStartTime = 0f;
    }

    /// <summary>
    /// End the game and quit the App. No escape/quit UI configured, quit the app is called from the last scene's timeline.
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
    }
}
