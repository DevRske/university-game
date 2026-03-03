using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The transform that will be rotated. Defaults to the transform this script is attached to.")]
    [SerializeField] private Transform _targetTransform;

    [Tooltip("The camera used to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
    [SerializeField] private Camera _referenceCamera;

    [Header("Configuration")]
    [Tooltip("Offset applied to the final rotation in degrees. Adjust if the sprite's default forward direction isn't the Unity default (Right/X-axis). Often -90 for Top-Down sprites facing 'Up'.")]
    [SerializeField] private float _rotationOffset = -90f;

    [Tooltip("If true, the character will only update its rotation when the mouse is within the game viewport bounds.")]
    [SerializeField] private bool _restrictToViewport = true;

    [Tooltip("If true, the rotation applies instantly. If false, it smoothly rotates over time.")]
    [SerializeField] private bool _isInstantRotation = true;

    [Tooltip("The speed of rotation if Is Instant Rotation is false.")]
    [SerializeField] private float _rotationSpeed = 15f;

    // Internal pre-calculated state and constants
    private Vector3 _mouseWorldPosition;
    private const float MIN_LOOK_DISTANCE_SQR = 0.001f;

    private void Awake()
    {
        // Cache reference internally just to be safe, avoiding GetComponent or repeated property access
        if (_targetTransform == null)
        {
            _targetTransform = transform;
        }
    }

    private void Start()
    {
        // Defensive Check: Automatically grab Camera.main if no camera is assigned
        if (_referenceCamera == null)
        {
            _referenceCamera = Camera.main;

            if (_referenceCamera == null)
            {
                Debug.LogError("[PlayerRotation] Reference Camera is missing and Camera.main could not be found. Script will be disabled.", this);
                enabled = false;
            }
        }
    }

    private void Update()
    {
        ProcessRotation();
    }

    /// <summary>
    /// Reads input and applies rotation to the target transform, keeping it within viewport boundaries if required.
    /// </summary>
    private void ProcessRotation()
    {
        if (Mouse.current == null) return;

        // Capture standard mouse position using new Input System
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();

        // Success criteria requirement: cursor must be within bounds of the game viewport
        if (_restrictToViewport && !IsMouseWithinViewport(mouseScreenPosition))
        {
            return;
        }

        // Convert Screen to World Space
        _mouseWorldPosition = _referenceCamera.ScreenToWorldPoint(mouseScreenPosition);
        
        // Set the World Z to the Target's Z to ensure mathematically planar alignment
        _mouseWorldPosition.z = _targetTransform.position.z;

        // Determine direction vector
        Vector3 lookDirection = _mouseWorldPosition - _targetTransform.position;

        // Avoid attempting to rotate if the mouse is directly over the pivot center, preventing jitter
        if (lookDirection.sqrMagnitude > MIN_LOOK_DISTANCE_SQR)
        {
            // Calculate atan2 rotation across the Z axis
            float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            
            // Add the configuration offset based on asset orientation
            targetAngle += _rotationOffset;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

            if (_isInstantRotation)
            {
                _targetTransform.rotation = targetRotation;
            }
            else
            {
                _targetTransform.rotation = Quaternion.Slerp(_targetTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Mathematically verifies if the mouse cursor coordinates are within the local bounds of the assigned camera's viewport.
    /// </summary>
    /// <param name="mousePosition">Current Input.mousePosition value.</param>
    /// <returns>True if the coordinates fall between (0,0) and (1,1) inclusive.</returns>
    private bool IsMouseWithinViewport(Vector3 mousePosition)
    {
        Vector3 viewportPoint = _referenceCamera.ScreenToViewportPoint(mousePosition);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
               viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }
}
