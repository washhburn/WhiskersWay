using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID; 
    public string questName;
    public string description;
    public List<QuestObjective> objectives;
    public Item rewardItem;

    private void OnValidate()
    {
        // Ensure the quest ID is unique
        if (string.IsNullOrEmpty(questID))
        {
            questID =  questName + Guid.NewGuid().ToString();
        }
    }
}

[Serializable]
public class QuestObjective
{
    public string objectiveID;
    public EnemyType targetEnemy;
    public string locationID;
    public string npcID;
    public string itemName;
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;
    public int sceneIndex;

    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType { CollectItem, DefeatEnemy, ReachLocation, TalkNPC }

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;
    public bool rewardGiven = false;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                targetEnemy = obj.targetEnemy,
                locationID = obj.locationID,
                npcID = obj.npcID,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0,
                itemName = obj.itemName,
                sceneIndex = obj.sceneIndex
            });
        }
    }

    public bool IsQuestCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;

}
