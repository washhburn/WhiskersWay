using UnityEngine;
using UnityEngine.SceneManagement;

public class Item : MonoBehaviour
{
    public string itemName;
    public Sprite icon;
    public bool isQuestItem;
    public int ID;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryController inventory = FindAnyObjectByType<InventoryController>();
            inventory.AddItem(this);

            if (isQuestItem)
            {
                int scene = SceneManager.GetActiveScene().buildIndex;
                QuestManager.Instance.RegisterItemCollect(itemName, scene);
            }

            Destroy(gameObject);
        }
    }
}
