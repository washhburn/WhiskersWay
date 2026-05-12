using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    public string locationID;


    //method to register player reaching a location related to quest objective
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        QuestManager.Instance.RegisterLocation(locationID);
    }
}
