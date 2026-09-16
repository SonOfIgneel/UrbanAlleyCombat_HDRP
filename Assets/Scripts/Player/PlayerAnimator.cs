using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speedDampTime = 0.1f;

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int FireParameter = Animator.StringToHash("Fire");

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (animator == null || characterController == null)
            return;

        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        animator.SetFloat(SpeedParameter, velocity.magnitude, speedDampTime, Time.deltaTime);
    }

    public void PlayFire()
    {
        if (animator != null)
            animator.SetTrigger(FireParameter);
    }
}
