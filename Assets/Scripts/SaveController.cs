using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private static SaveController instance;
    public static SaveController Instance;
    public Quest mainQuest;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Instance = this;
        DontDestroyOnLoad(gameObject); //Keep this object alive across scene loads
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        inventoryController = FindAnyObjectByType<InventoryController>();

        QuestManager.Instance.AcceptQuest(mainQuest);
    }

    public void SaveGame() //method to save game data
    {
        if (inventoryController == null)
        {
            inventoryController = FindAnyObjectByType<InventoryController>();
        }

        if (inventoryController == null) return;
        
        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();

        SaveData saveData = new SaveData
        {
            playerLives = ph != null ? ph.lives : 3,
            playerStamina = ph != null ? ph.stamina : 100f,
            currentScene = SceneManager.GetActiveScene().buildIndex,
            lastCheckpointID = currentCheckpointID,
            checkpointPosition = currentCheckpointPosition,
            inventorySaveData = inventoryController.GetInventoryItems(),
            questSaveData = QuestManager.Instance.GetQuestSaveData()
        };
        
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame() //method to load saved game data
    {
        if (!File.Exists(saveLocation))
        {
            SaveGame(); //Om ingen savefile hittas, spara ny savefile
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        currentCheckpointID = saveData.lastCheckpointID;
        currentCheckpointPosition = saveData.checkpointPosition;
        lastSavedScene = saveData.currentScene;

        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
        {
            ph.lives = saveData.playerLives;
            ph.stamina = saveData.playerStamina;
        }

        inventoryController = FindAnyObjectByType<InventoryController>();
        inventoryController.SetInventoryItems(saveData.inventorySaveData);

        QuestManager.Instance.LoadQuestSaveData(saveData.questSaveData);
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
        StartCoroutine(LoadGameDelayed());
    }

    private IEnumerator LoadGameDelayed()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;


        inventoryController = FindAnyObjectByType<InventoryController>();
        if (inventoryController == null) yield break;
        LoadGame();
    }

    public Vector3 currentCheckpointPosition;
    public int currentCheckpointID = -1;
    private int lastSavedScene;

    public void SetCheckpoint(int id, Vector3 position)
    {
        currentCheckpointID = id;
        currentCheckpointPosition = position;
        lastSavedScene = SceneManager.GetActiveScene().buildIndex;
        SaveGame();
    }

    public async void RestartFromCheckpoint()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        if (lastSavedScene != currentScene) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        ph.Respawn(currentCheckpointPosition);

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeIn();
    }

    public void ResetSave()
    {
        currentCheckpointID = -1;
        currentCheckpointPosition = Vector3.zero;
        lastSavedScene = 1;
        if (File.Exists(saveLocation)) File.Delete(saveLocation);
    }
}
