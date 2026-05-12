using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private Collider2D hitbox;
    public float damage = 1f;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false; // Inaktivera hitboxen vid start
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return; // Endast reagera på kollisioner med fiender
        Enemy enemy = collision.GetComponent<Enemy>();
        enemy?.TakeDamage(damage); // Anropa metoden för att ta skada på fienden
    }

    public void Enable() => hitbox.enabled = true; // Aktivera hitboxen
    public void Disable() => hitbox.enabled = false; // Inaktivera hitboxen
}
