using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Powerups : MonoBehaviour
{
    public enum itemType
    {
        DamageBoost,
        SpeedBoost,
        ShieldBoost,
    }

    public itemType type;
    public float boostAmount = 1f;
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        switch(type)
        {
            case itemType.DamageBoost:
                AttackHitbox hitbox = other.GetComponentInChildren<AttackHitbox>();
                if (hitbox != null)
                    hitbox.damage += boostAmount;
                break;

            case itemType.SpeedBoost: 
                PlayerMovement movement = other.GetComponent<PlayerMovement>();
                if (movement != null)
                    PowerupManager.Instance.ApplyPowerup(
                        () => { movement.walkSpeed += boostAmount; movement.runSpeed += boostAmount; },
                        () => { movement.walkSpeed -= boostAmount; movement.runSpeed -= boostAmount; },
                        duration);   
                break;

            case itemType.ShieldBoost: 
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.maxLives += (int)boostAmount;
                    health.Heal((int)boostAmount);
                }  
                break;
        }
        Destroy(this.gameObject);
    }
}
