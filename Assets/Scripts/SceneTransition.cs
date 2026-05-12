using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Required items")]
    public string requiredItemName;
    public int requiredAmount = 5;

    [Header("Next scene")]
    public int nextSceneBuildIndex;

    private InventoryController inventory;
    private SaveController saveController;

    void Start()
    {
        inventory = FindAnyObjectByType<InventoryController>();
        saveController = FindAnyObjectByType<SaveController>();
    }

    void Update()
    {
        bool isOpen = inventory.GetQuestItemCount(requiredItemName) >= requiredAmount;

        GetComponent<Collider2D>().isTrigger = isOpen;
    }

    async void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ScreenFader fader = FindAnyObjectByType<ScreenFader>();
        if (fader != null) await fader.FadeOut();

        InventoryController inventory = FindAnyObjectByType<InventoryController>();
        if (inventory != null)
        {
            for (int i = 0; i < 5; i++) inventory.RemoveQuestItem("collectibleItem");
        }

        saveController.SaveGame();
        SceneManager.LoadScene(nextSceneBuildIndex);
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
        inventory = FindAnyObjectByType<InventoryController>();
        saveController = FindAnyObjectByType<SaveController>();
    }

}
