using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Permit to visually activate or deactivate some Scene's gameObjects.
/// Serve as a functions relay for each scene to call the ScenesManager through timeline signals (via Signal receiver).
/// As ScenesManager is instanciated in Startup scene, it cannot be referenced directly as a Signal receiver in other timeline scenes.
/// </summary>
public class SceneEvents : MonoBehaviour
{
    /// <summary>
    /// Activated or deactivated object depending on chosen Timeline and Timeline progression.
    /// </summary>
    [System.Serializable]
    public class VisibleObject
    {
        public string ObjectName;
        public GameObject GameObject;
    }

    // Inspector
    [Tooltip("List of all activated and deactivated objects depending on Timeline.")]
    [SerializeField] private List<VisibleObject> _visibleObjects;

    // Methods
    public void ChangeScene(string sceneName)
    {
        ScenesManager.Instance.ChangeScene(sceneName);
    }

    public void SetNextTimelineID(int timelineID)
    {
        ScenesManager.Instance.SetNextTimelineID(timelineID);
    }

    public void SetNextTimelineStartTime(float timelineStartTime)
    {
        ScenesManager.Instance.SetNextTimelineStartTime(timelineStartTime);
    }

    public void ActivateObject(string gameObjectName)
    {
        foreach(VisibleObject sceneObject in _visibleObjects)
        {
            if (sceneObject.ObjectName == gameObjectName)
            {
                Debug.Log("Activate " + sceneObject.ObjectName);
                sceneObject.GameObject.SetActive(true);
            }
        }
    }

    public void DeactivateObject(string gameObjectName)
    {
        foreach(VisibleObject sceneObject in _visibleObjects)
        {
            if (sceneObject.ObjectName == gameObjectName)
            {
                sceneObject.GameObject.SetActive(false);
            }
        }
    }

    public void QuitApp()
    {
        ScenesManager.Instance.QuitApp();
    }
}
