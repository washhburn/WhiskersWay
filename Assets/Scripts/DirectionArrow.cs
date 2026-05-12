using UnityEngine;
using UnityEngine.SceneManagement;

public class DirectionArrow : MonoBehaviour
{
    public Transform target;
    public string requiredItemname = "collectibleItem";
    public int requiredAmount = 5;

    private InventoryController inventory;
    private GameObject arrow;

    void Start()
    {
        inventory = FindAnyObjectByType<InventoryController>();
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (inventory == null) return;
        bool isComplete = inventory.GetQuestItemCount(requiredItemname) >= requiredAmount;
        GetComponent<SpriteRenderer>().enabled = isComplete;

        //rotate the arrow to show where the transition point is
        if (isComplete && target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle -= 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneTransition st = FindAnyObjectByType<SceneTransition>();
        if (st != null) target = st.transform;

        inventory = FindAnyObjectByType<InventoryController>();
    }
}
