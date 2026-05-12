using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<QuestProgress> activeQuests = new ();

    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        questUI = FindAnyObjectByType<QuestUI>();
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;

        activeQuests.Add(new QuestProgress(quest));

        GetQuestUI()?.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) => activeQuests.Exists(q => q.quest.questID == questID);


    public void RegisterEnemyKill(EnemyType enemyType)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (!obj.IsCompleted)
                {
                    if (obj.type == ObjectiveType.DefeatEnemy && obj.targetEnemy == enemyType)
                    {
                        obj.currentAmount++;
                        break;
                    }
                }
            }
            if (quest.IsQuestCompleted) GiveQuestReward(quest.QuestID);
        }
        GetQuestUI()?.UpdateQuestUI();
    }

    public void RegisterLocation(string locationID)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.IsCompleted) continue;
                if (obj.type == ObjectiveType.ReachLocation && obj.locationID == locationID)
                { 
                    obj.currentAmount = obj.requiredAmount;
                    break; 
                }
            }
            if (quest.IsQuestCompleted) GiveQuestReward(quest.QuestID);
        }
        GetQuestUI()?.UpdateQuestUI();
    }

    public void RegisterNPCTalk (string npcID)
    {

        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.IsCompleted) continue;

                if (obj.type == ObjectiveType.TalkNPC && obj.npcID == npcID)
                {
                    obj.currentAmount = obj.requiredAmount;
                    break;
                }
            }
            if (quest.IsQuestCompleted) GiveQuestReward(quest.QuestID);
        }
        GetQuestUI()?.UpdateQuestUI();
    }
    public void RegisterItemCollect(string itemName, int sceneIndex)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.IsCompleted) continue;

                if (obj.type == ObjectiveType.CollectItem && 
                    obj.itemName == itemName && 
                    obj.sceneIndex == sceneIndex)
                {
                    obj.currentAmount++;
                    break;
                }
            }
            if (quest.IsQuestCompleted) GiveQuestReward(quest.QuestID);
        }
        GetQuestUI()?.UpdateQuestUI();
    }

    public bool IsQuestCompleted(string questID)
    {
        var quest = GetQuest(questID);
        return quest != null && quest.IsQuestCompleted;
    }

    public QuestProgress GetQuest(string questID) => activeQuests.Find(q => q.quest.questID == questID);

    private QuestUI GetQuestUI()
    {
        if (questUI == null) questUI = FindAnyObjectByType<QuestUI>();
        return questUI;
    }

    public List<QuestSaveData> GetQuestSaveData()
    {
        var list = new List<QuestSaveData>();
        foreach (var qp in activeQuests)
        {
            var objData = new List<ObjectiveSaveData>();
            foreach (var obj in qp.objectives)
            {
                objData.Add(new ObjectiveSaveData
                {
                    objectiveID = obj.objectiveID,
                    currentAmount = obj.currentAmount
                });
            }
            list.Add(new QuestSaveData { questID = qp.QuestID, objectives = objData });
        }
        return list;
    }

    public void LoadQuestSaveData(List<QuestSaveData> saveDataList)
    {
        if (saveDataList == null) return;

        foreach (var saved in saveDataList)
        {
            var qp = activeQuests.Find (q => q.QuestID == saved.questID);
            if (qp == null) continue;
            foreach (var savedObj in saved.objectives)
            {
                var obj = qp.objectives.Find(o => o.objectiveID == savedObj.objectiveID);
                if (obj != null) obj.currentAmount = savedObj.currentAmount;
            }
        }
    }

    public void GiveQuestReward(string questID)
    {
        QuestProgress qp = GetQuest(questID);
        if (qp == null || !qp.IsQuestCompleted) return;
        if (qp.rewardGiven) return; //ensure quest item is not given several times

        InventoryController inventory = FindAnyObjectByType<InventoryController>();

        foreach(var obj in qp.objectives)
        {
            if (obj.type == ObjectiveType.CollectItem && !string.IsNullOrEmpty(obj.itemName))
            {
                inventory?.RemoveItemByName(obj.itemName, obj.requiredAmount);
            }
        }

        if (qp.quest.rewardItem != null) inventory?.AddItem(qp.quest.rewardItem.GetComponent<Item>());
        qp.rewardGiven = true;
    }
}
