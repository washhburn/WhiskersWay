using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (QuestManager.Instance == null) return;
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        //destroy existing entries in quest list
        for (int i = questListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(questListContent.GetChild(i).gameObject);
        }

        //build new entries
        foreach(var quest in QuestManager.Instance.activeQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");
            TMP_Text descriptionText = entry.transform.Find("ObjectiveList/Description").GetComponent<TMP_Text>();

            questNameText.text = quest.quest.questName;
            descriptionText.text = quest.quest.description;

            if (quest.IsQuestCompleted)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = "Quest Completed!";
                objText.color = Color.brown;
            }
            else
            {
                foreach (var objective in quest.objectives)
                {
                    if (objective.sceneIndex == currentScene && !objective.IsCompleted)
                    {
                        GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                        TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                        objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
                        break; //only show the first incomplete objective
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (QuestManager.Instance == null) return;
        UpdateQuestUI();
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
            Transform content = ui.transform.Find("Menu/Pages/QuestPage/QuestScrollView/Viewport/Content");
            if (content != null) questListContent = content;
        }
    }

}
