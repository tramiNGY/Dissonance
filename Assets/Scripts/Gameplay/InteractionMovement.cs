using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


/// <summary>
/// Perform Interactable Movement associated with Press interaction and Grab around pivot interaction.
/// </summary>
public class InteractionMovement : MonoBehaviour
{
    /// <summary>
    /// Define the axis and direction of movement.
    /// </summary>
    public enum MovementAxis
    {
        X,
        Y,
        Z,
        NegativeX,
        NegativeY,
        NegativeZ
    }

    // Inspector
    [Header("Press Movement")]
    [Tooltip("Direction of interactable when moving after being pressed.")]
    [SerializeField] private MovementAxis _directionAxis;
    [Tooltip("Distance from interactable initial position to its final position after being pressed.")]
    [SerializeField] private float _pressDistance;
    [Tooltip("Speed at which the interactable move during Press movement.")]
    [SerializeField] private float _pressSpeed;
    [Header("Grab Rotation Movement")]
    [Tooltip("Pivot Axis around which interactable is rotating while being grabbed.")]
    [SerializeField] private MovementAxis _rotationAxis;
    [Tooltip("Interactable that is rotating around pivot.")]
    [SerializeField] private Transform _interactableTransform;

    // Fields
    private Vector3 _initialPos;
    private Vector3 _targetPos;
    
    private bool _isPressing = false;
    private bool _isReturnPressing = false;
    
    private bool _isRotating = false;
    private IXRSelectInteractor _selectInteractor;
    private IXRSelectInteractable _selectInteractable;
    private Vector3 _selectInteractorPos;
    private float _rotationAngleDegrees;
    private float _initialInteractorAngle;
    private float _rotationDelta;
    
    // Unity Lifecycle
    void Start()
    {
        // Initialize Interactable Press positions.
        _initialPos = transform.position;
        _targetPos = _initialPos + _pressDistance * GetMovementAxis(_directionAxis);
    }

    void Update()
    {
        if (_isPressing || _isReturnPressing)
        {
            UpdatePressMovement();
        }

        if (_isRotating)
        {
            UpdatePivotMovement();
        }
    }

    // Methods
    /// <summary>
    /// Get normalized World Space direction vector corresponding to specified movement axis.
    /// </summary>
    private Vector3 GetMovementAxis(MovementAxis axis)
    {
        switch (axis)
        {
            case MovementAxis.X:
                return transform.right;

            case MovementAxis.NegativeX:
                return -transform.right;

            case MovementAxis.Y:
                return transform.up;

            case MovementAxis.NegativeY:
                return -transform.up;

            case MovementAxis.Z:
                return transform.forward;

            case MovementAxis.NegativeZ:
                return -transform.forward;

            default:
                return Vector3.zero;
        }
    }

    // Press Interaction Movement
    /// <summary>
    /// Called when XR Simple Interactable starts detecting poking interaction (Select Entered).
    /// </summary>
    public void PressMovement(SelectEnterEventArgs args)
    {
        _isPressing = true;
    }

    /// <summary>
    /// Update Interactable's position when pressed with a simplified back and forth motion in the direction of Player's Poke interaction.
    /// </summary>
    private void UpdatePressMovement()
    {
        // From Initial Pos to Target Pos.
        if (_isPressing)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _pressSpeed * Time.deltaTime);
            if (transform.position == _targetPos)
                {
                    _isPressing = false;
                    _isReturnPressing = true;
                }
        }
        // From Target Pos back to Initial Pos.
        if (_isReturnPressing)
        {
            transform.position = Vector3.MoveTowards(transform.position, _initialPos, _pressSpeed * Time.deltaTime);
            if (transform.position == _initialPos)
            {
                _isReturnPressing = false;
            }
        }
    }

    // Grab Interaction Movement
    // Pivot Rotation
    /// <summary>
    /// Called when XR Grab Interactable starts detecting grabbing interaction (Select Entered).
    /// </summary>
    /// <param name="args">Event data from XR Interaction Toolkit associated with the event when an interactor initiates selecting an interactable.</param>
    public void PivotMovement(SelectEnterEventArgs args)
    {
        _isRotating = true;
        _selectInteractor = args.interactorObject; // Interactor is the Player grabbing interactable object.
        _selectInteractable = args.interactableObject; // Interactable is the object grabbed and rotating around pivot.
        _initialInteractorAngle = GetInteractorAngle();
    }

    /// <summary>
    /// Update Interactable's rotation to follow Interactor's Player hand rotation when grabbed around pivot axis. Rotation can go both directions around same axis.
    /// </summary>
    private void UpdatePivotMovement()
    {
        _rotationAngleDegrees = GetInteractorAngle();
        _rotationDelta = _rotationAngleDegrees - _initialInteractorAngle;
        switch (_rotationAxis)
        {
            case MovementAxis.X:
            case MovementAxis.NegativeX:
                transform.eulerAngles += new Vector3(_rotationDelta, 0, 0);
                _interactableTransform.eulerAngles += new Vector3(_rotationDelta, 0, 0);
                break;

            case MovementAxis.Y:
            case MovementAxis.NegativeY:
                transform.eulerAngles += new Vector3(0, _rotationDelta, 0);
                _interactableTransform.eulerAngles += new Vector3(0, _rotationDelta, 0);
                break;

            case MovementAxis.Z:
            case MovementAxis.NegativeZ:
                transform.eulerAngles += new Vector3(0, 0, _rotationDelta);
                _interactableTransform.eulerAngles += new Vector3(0, 0, _rotationDelta);
                break;
        }
        // Debug.Log($"Current Interactor Angle: {_rotationAngleDegrees}, Initial Interactor Angle: {_initialInteractorAngle}, Rotation delta: {_rotationDelta}");
    }

    /// <summary>
    /// Called when XR Grab Interactable stops detecting grabbing interaction (Select Exited).
    /// </summary>
    public void StopPivotMovement(SelectExitEventArgs args)
    {
        _isRotating = false;
    }

    /// <summary>
    /// Calculate Angle in degrees between Interactor's hand and pivot when grabbing Interactable.
    /// </summary>
    /// <returns>Interactor angle in degrees relative to Interactable's local space.</returns>
    private float GetInteractorAngle()
    {
        // Convert Interactor's hand attach point position from world space to interactable's local space.
        _selectInteractorPos = transform.InverseTransformPoint(_selectInteractor.GetAttachTransform(_selectInteractable).position);
        float angle;
        // Calculate the angle in the plane perpendicular to the selected pivot axis.
        // Atan2() used instead of Atan() to preserve direction of the angle.
        switch (_rotationAxis)
        {
            case MovementAxis.X: // Rotation around X axis.
                angle = Mathf.Atan2(_selectInteractorPos.y, _selectInteractorPos.z); // Calculate angle in the YZ plane.
                
                break;

            case MovementAxis.NegativeX:
                angle = -Mathf.Atan2(_selectInteractorPos.y, _selectInteractorPos.z);
                break;

            case MovementAxis.Y:
                angle = Mathf.Atan2(_selectInteractorPos.x, _selectInteractorPos.z);
                break;

            case MovementAxis.NegativeY:
                angle = -Mathf.Atan2(_selectInteractorPos.x, _selectInteractorPos.z);
                break;

            case MovementAxis.Z:
                angle = Mathf.Atan2(_selectInteractorPos.y, _selectInteractorPos.x);
                break;

            case MovementAxis.NegativeZ:
                angle = -Mathf.Atan2(_selectInteractorPos.y, _selectInteractorPos.x);
                break;

            default:
                angle = 0f;
                break;
        }
        return angle * Mathf.Rad2Deg;
    }
}
