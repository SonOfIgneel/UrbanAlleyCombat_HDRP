using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVerticalSpeed = -2f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
    }

    private void Update()
    {
        Vector2 input = moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
        if (horizontalMovement.sqrMagnitude > 1f)
            horizontalMovement.Normalize();

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedVerticalSpeed;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalMovement * moveSpeed;
        velocity.y = verticalVelocity;

        CollisionFlags collisions = characterController.Move(velocity * Time.deltaTime);
        if ((collisions & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            verticalVelocity = groundedVerticalSpeed;
    }
}
