using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject statCanvas;

    [Header("Statpanel")]
    public Image healthBar;
    public Image staminaBar;
    public TMP_Text questProgressText;

    private PlayerHealth player;

    void Start()
    {
        if (menuCanvas != null) menuCanvas.SetActive(false);
        if (statCanvas != null) statCanvas.SetActive(true);
        player = FindAnyObjectByType<PlayerHealth>();
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateStaminaUI();
        UpdateQuestProgressUI();
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isOpen = !menuCanvas.activeSelf;
            menuCanvas.SetActive(isOpen);
            statCanvas.SetActive(!isOpen);
        }
    }

    public void UpdateHealthUI()
    {
        if (player == null) player = FindAnyObjectByType<PlayerHealth>();
        if (player == null || healthBar == null) return;
        healthBar.fillAmount = (float)player.lives / player.maxLives;
    }
    public void UpdateStaminaUI()
    {
        if (player == null) player = FindAnyObjectByType<PlayerHealth>();
        if (player == null || staminaBar == null) return;
        if (staminaBar != null) staminaBar.fillAmount = player.stamina / player.maxStamina;
    }

    public void UpdateQuestProgressUI()
    {
        if (questProgressText == null) return;

        InventoryController inventory = FindAnyObjectByType<InventoryController>();
        if (inventory == null) return;

        int collected = inventory.GetQuestItemCount("collectibleItem");
        int required = 5;

        questProgressText.text = $"Quest items: {collected}/{required}";
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
        if (scene.buildIndex == 0) return;

        GameObject ui = GameObject.Find("UI");

        if (ui != null)
        {
            menuCanvas = ui.transform.Find("Menu")?.gameObject;
            statCanvas = ui.transform.Find("StatPanel")?.gameObject;
        }

        if (statCanvas != null)
        {
            statCanvas.SetActive(true); 
            healthBar = GameObject.Find("HealthBar")?.GetComponent<Image>();
            staminaBar = GameObject.Find("StaminaBar")?.GetComponent<Image>();
            questProgressText = GameObject.Find("CollectProgress")?.GetComponent<TMP_Text>();
        }

        if (menuCanvas != null) menuCanvas.SetActive(false);

        player = FindAnyObjectByType<PlayerHealth>();
    }

}
