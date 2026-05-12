using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }

    public void ApplyPowerup(System.Action apply, System.Action reset, float duration)
    {
        apply();
        StartCoroutine(ResetAfterTime(reset, duration));
    }

    private IEnumerator ResetAfterTime(System.Action reset, float duration)
    {
        yield return new WaitForSeconds(duration);
        reset?.Invoke();
    }
}
