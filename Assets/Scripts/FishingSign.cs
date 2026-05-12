using System.Collections;
using UnityEngine;

public class FishingSign : MonoBehaviour, IInteractable
{
    [Header("Fishing Settings")]
    public GameObject[] fishPrefabs;
    public GameObject[] trashPrefabs;

    [Range(0f, 1f)]
    public float fishChance = 0.7f; // 70% chans att få en fisk, 30% chans att få skräp

    public float spawnDistance = 1.2f;
    public LayerMask obstacleLayer;

    public float minFishingTime = 1f;
    public float maxFishingTime = 3f;

    private bool isFishing = false;
    private Sign sign;

    private void Awake()
    {
        sign = GetComponent<Sign>();
    }

    public void Interact()
    {
        //blockera om popup är öppen
        if (sign != null && sign.IsShowing) return;
        
        if (!isFishing) StartCoroutine(FishingRoutine());
    }

    private IEnumerator FishingRoutine()
    {
        isFishing = true;

        PlayerInteraction playerInteraction = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteraction>();
        PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        playerInteraction?.SetPromptText("Fishing...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            isFishing = false;
            yield break;
        }

        //spelar attack-animation när player fiskar
        bool animDone = false;
        playerMovement?.StartFishing(() =>
        {
            animDone = true;
        });

        yield return new WaitUntil(() => animDone);

        //random tid som player måste vänta för att få en fisk eller skräp
        float waitTime = Random.Range(minFishingTime, maxFishingTime);
        yield return new WaitForSeconds(waitTime);

        Vector2 playerPos = player.transform.position;

        Vector2[] directions = new Vector2[]
        {
            Vector2.right,
            Vector2.left,
            Vector2.up,
            Vector2.down
        };

        bool catchFish = Random.value < fishChance;
        int amount = 1;
        GameObject[] pool = catchFish ? fishPrefabs : trashPrefabs;

        if (pool.Length > 0)
        {
            int spawned = 0;
            foreach (Vector2 dir in directions)
            {
                if (spawned >= amount) break;

                Vector2 spawnPos = playerPos + dir * spawnDistance;
                Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.3f, obstacleLayer);

                if (hit == null)
                {
                    GameObject prefab = pool[Random.Range(0, pool.Length)];
                    Instantiate(prefab, spawnPos, Quaternion.identity);
                    spawned++;
                }
            }

        }

        playerInteraction?.SetPromptText("Press F to fish again");
        isFishing = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteraction pi = other.GetComponent<PlayerInteraction>();
            pi?.SetPromptText("Press F to interact");
            isFishing = false;
        }
    }
}
