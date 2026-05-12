using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public int lives = 3; //antalet liv som spelaren har
    public int maxLives = 6;
    public float invincibilityTime = 1f; //tiden i sekunder som spelaren är osårbar efter att ha tagit skada
    private bool isInvincible = false; //en flagga som indikerar om spelaren är osårbar eller inte

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    private float regenTimer = 0f;
    public float regenDelay = 1f;
    public float regenRate = 15f;
    public bool canRun = true;

    private SpriteRenderer[] spriteRenderers;
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        RegenStamina();
    }

    public void LoseLife(int amount) //en metod som hanterar när spelaren förlorar ett liv
    {
        if (isInvincible) return;
        lives -= amount;

        //anropar Hurt-metoden i PlayerMovement för att starta skadesequensen
        if (movement != null) movement.Hurt();

        if (lives <= 0)
        {
            lives = 0;
            Die();
            return;
        }
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibilityTime)
        {
            ToggleSprites();
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
        ResetSprites();
        isInvincible = false;
    }

    private void ToggleSprites()
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr == null || !sr.enabled) continue;
            Color c = sr.color;
            c.a = (c.a == 1f) ? 0.3f : 1f;
            sr.color = c;
        }
    }

    private void ResetSprites()
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    private void Die()
    {
        //anropar Die-metoden i PlayerMovement för att starta dödssekvensen
        if (movement != null) movement.Die();
        Invoke(nameof(OnDeathSequenceEnd), 1.5f);
    }

    private void OnDeathSequenceEnd()
    {
        GameOverMenu gameOverMenu = FindAnyObjectByType<GameOverMenu>(FindObjectsInactive.Include);
        if (gameOverMenu != null) gameOverMenu.Show();
    }

    public bool TryUseStamina(float amount)
    {
        if (!canRun || stamina <= 0f) return false;

        stamina -= amount;
        regenTimer = 0f;

        if (stamina <= 0f)
        {
            stamina = 0f;
            canRun = false;
        }
        return true;
    }

    private void RegenStamina()
    {
        regenTimer += Time.deltaTime;
        if (regenTimer < regenDelay) return;

        stamina += regenRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        if (stamina > maxStamina * 0.3f) canRun = true;
    }

    public void Heal(int amount)
    {
        lives += amount;
        lives = Mathf.Clamp(lives, 0, maxLives);
    }

    public void Respawn(Vector3 position)
    {
        CancelInvoke(nameof(OnDeathSequenceEnd));

        lives = maxLives;
        stamina = maxStamina;
        transform.position = position;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        ResetSprites();
        isInvincible = false;
        if (movement != null) movement.Revive();
        gameObject.SetActive(true);
    }
}
