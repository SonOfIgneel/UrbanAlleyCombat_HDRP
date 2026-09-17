using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class EndTrigger : MonoBehaviour
{
    [SerializeField] private ScenarioManager scenarioManager;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (scenarioManager != null &&
            other.GetComponentInParent<FPSPlayerController>() != null)
        {
            scenarioManager.SetPlayerInEndArea(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (scenarioManager != null &&
            other.GetComponentInParent<FPSPlayerController>() != null)
        {
            scenarioManager.SetPlayerInEndArea(false);
        }
    }
}
