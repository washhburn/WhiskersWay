using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID;
    private bool activated;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (activated) return;

        activated = true;

        SaveController.Instance.SetCheckpoint(checkpointID, transform.position);
    }
}
