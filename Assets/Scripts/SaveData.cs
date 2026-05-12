using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class SaveData
{
    public int playerLives;
    public float playerStamina;
    public int currentScene;
    public int lastCheckpointID;
    public Vector3 checkpointPosition;
    public List<InventorySaveData> inventorySaveData;
    public List<QuestSaveData> questSaveData;
}

[System.Serializable]
public class QuestSaveData
{
    public string questID;
    public List<ObjectiveSaveData> objectives;
}

[System.Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;
}
