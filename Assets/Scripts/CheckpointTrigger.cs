using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            RaceManager.Instance.PlayerPassedCheckpoint(this);
        }
    }
}
