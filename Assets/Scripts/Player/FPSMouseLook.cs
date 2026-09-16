using UnityEngine;
using UnityEngine.InputSystem;

public class FPSMouseLook : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float sensitivity = 0.08f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-85f, 85f);

    private float pitch;
    private bool cursorLocked;

    private void Awake()
    {
        if (cameraPivot != null)
            pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
    }

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();

        LockCursor();
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();
    }

    private void Update()
    {
        HandleCursor();

        if (!cursorLocked || lookAction == null || playerTransform == null || cameraPivot == null)
            return;

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        playerTransform.Rotate(Vector3.up, lookInput.x * sensitivity, Space.Self);

        pitch -= lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleCursor()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
            return;
        }

        if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
