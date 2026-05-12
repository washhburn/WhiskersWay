using System.Collections;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 spawnPos = transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = spawnPos;
        }
        player.transform.position = spawnPos;
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return null;
        yield return null;

        ScreenFader fader = FindAnyObjectByType<ScreenFader>();

        if (fader != null) _ = fader.FadeIn();
    }
}
