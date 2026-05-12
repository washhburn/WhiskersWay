using UnityEngine;

public class FishObject : MonoBehaviour
{
    public int healAmount = 1; //standard heal amount of a fish

    void Start()
    {
        Destroy(gameObject, 5f); // Fish/trash is destroyed after 5 sec if not picked up
    }


    //method for player to pick up item that's been fished
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.Heal(healAmount);
            }
            Destroy(gameObject);
        }
    }
}
