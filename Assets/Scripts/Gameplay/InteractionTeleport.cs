using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Teleport gameObject in Scene WorldSpace to another position and rotation.
/// Use to Teleport Player within the same scene to different locations.
/// </summary>
public class InteractionTeleport : MonoBehaviour
{
    [System.Serializable]
    public class TeleportConfiguration
    {
        public GameObject TeleportObject;
        public Transform TeleportPoint;
    }

    // Inspector
    [SerializeField] private List<TeleportConfiguration> _teleportConfigurations;

    // Methods
    /// <summary>
    /// Teleport gameObject in World Space to the teleport point's global coordinates specified by the timeline signal.
    /// </summary>
    public void TeleportObjectToPoint(Transform teleportPoint)
    {
        foreach(TeleportConfiguration config in _teleportConfigurations)
        {
            if (config.TeleportPoint == teleportPoint)
            {
                config.TeleportObject.transform.position = teleportPoint.position;
                config.TeleportObject.transform.rotation = teleportPoint.rotation;
                Debug.Log($"{config.TeleportObject} has been teleported to point {teleportPoint}");
            }
        }
    }
}
