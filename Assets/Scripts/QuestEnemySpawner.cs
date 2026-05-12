using UnityEngine;

public class QuestEnemySpawner : MonoBehaviour
{
    public Quest quest;
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoint;
    private bool hasSpawn = false;

    void Update()
    {
        if (hasSpawn) return;
        if (QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsQuestActive(quest.questID))
        {
            SpawnEnemies();
            hasSpawn = true;
        } 
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < spawnPoint.Length; i++)
        {
            GameObject prefab = enemyPrefabs[i % enemyPrefabs.Length];
            Instantiate(prefab, spawnPoint[i].position, Quaternion.identity);
        }
    }
}
