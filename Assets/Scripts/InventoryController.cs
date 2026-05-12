using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private static InventoryController instance;
    
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    
    private List<Slot> slots = new List<Slot>();

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
    }

    void CreateSlots()
    {
        if (inventoryPanel == null) return;
        
        if (slots.Count > 0) return;
        
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, inventoryPanel.transform);
            Slot slot = slotObject.GetComponent<Slot>();
            slots.Add(slot);
        }
    }

    public void AddItem(Item item)
    {
        foreach (Slot slot in slots)
        {
            if(slot.currentItem == null)
            {

                GameObject itemObj = new GameObject(item.itemName);
                itemObj.transform.SetParent(slot.transform);

                Item itemData = itemObj.AddComponent<Item>();
                itemData.itemName = item.itemName;
                itemData.icon = item.icon;
                itemData.isQuestItem = item.isQuestItem;
                itemData.ID = item.ID;

                slot.SetItem(itemObj);
                return;
            }
        }
    }

    //List to save inventory data for saving/loading
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> saveData = new List<InventorySaveData>();
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].currentItem != null)
            {
                Item item = slots[i].currentItem.GetComponent<Item>();
                saveData.Add(new InventorySaveData 
                { 
                    itemID = item.ID, 
                    slotIndex = i 
                });
            }
        }
        return saveData;
    }


    // Method to load inventory items from saved data
    public void SetInventoryItems(List<InventorySaveData> saveDataList)
    {
        if (saveDataList == null || saveDataList.Count == 0) return;
        if (slots.Count == 0) return;
        
        ClearInventory();
        CreateSlots();
        
        foreach (var data in saveDataList)
        {
            if (data.slotIndex >= slots.Count) continue;

            GameObject prefab = itemDictionary.GetItemPrefab(data.itemID);
            if (prefab == null) continue;

            Item itemData = prefab.GetComponent<Item>();
            if (itemData == null) continue;

            GameObject itemObj = new GameObject(itemData.itemName);
            itemObj.transform.SetParent(slots[data.slotIndex].transform);

            RectTransform rt = itemObj.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(100, 100);

            Image img = itemObj.AddComponent<Image>();
            img.sprite = itemData.icon;

            Item newItem = itemObj.AddComponent<Item>();
            newItem.itemName = itemData.itemName;
            newItem.icon = itemData.icon;
            newItem.isQuestItem = itemData.isQuestItem;
            newItem.ID = itemData.ID;
            slots[data.slotIndex].SetItem(itemObj);
        }
    }

    //method to clear inventory
    void ClearInventory()
    {
        foreach (Slot slot in slots)
        {
            if (slot == null) continue;
            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.ClearSlot();
        }
        slots.Clear();
    }

    //method for quest item management
    public int GetQuestItemCount(string itemName)
    {
        int count = 0;
        foreach (Slot slot in slots)
        {
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.itemName == itemName) count++;
            }
        }
        return count;
    }

    public void RemoveQuestItem(string itemName)
    {
        foreach (Slot slot in slots)
        {
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item.itemName == itemName && item.isQuestItem)
                {
                    Destroy(slot.currentItem);
                    slot.ClearSlot();
                    return;
                }
            }
        }
    }

    public void RemoveItemByName (string itemName, int amount)
    {
        int removed = 0;
        foreach (Slot slot in slots)
        {
            if (removed >= amount) break;
            if (slot.currentItem == null) continue;
            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null && item.itemName == itemName)
            {
                Destroy(slot.currentItem);
                slot.ClearSlot();
                removed++;
            }
        }
    }


    //methods to save between scenes
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
        StartCoroutine(InitDelayed());
    }

    private IEnumerator InitDelayed()
    {
        yield return null;
        yield return null;

        GameObject ui = GameObject.Find("UI");

        if (ui != null)
        {
            Transform t = ui.transform.Find("Menu/Pages/InventoryPage");
            if (t != null) inventoryPanel = t.gameObject;
        }

        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryPage not found!");
            yield break;
        }
        
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        slots.Clear();
        CreateSlots();
    }
}
