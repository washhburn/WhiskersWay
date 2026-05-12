using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    private Vector2 movement;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public bool canRun = false;
    public int damage = 1;
    public float detectionRange = 5f;
    public float attackCooldown = 1.5f;
    public float lastAttack;

    [Header("Animation")]
    public AnimatedSpriteRenderer anim;

    [Header("Powerup drops")]
    public GameObject[] powerupPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.3f;


    [Header("Walk Animations")]
    public Sprite[] walkUp;
    public Sprite[] walkDown;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;

    [Header("Run Animations")]
    public Sprite[] runUp;
    public Sprite[] runDown;
    public Sprite[] runLeft;
    public Sprite[] runRight;

    [Header("Action Animations")]
    public Sprite[] attackUp;
    public Sprite[] attackDown;
    public Sprite[] attackLeft;
    public Sprite[] attackRight;
    public Sprite[] death;

    private enum MoveState { Walk, Run, Attack, Hurt, Death }
    private enum Direction { Up, Down, Left, Right }

    private MoveState state;
    private MoveState lastState;
    private Direction direction;
    private Direction lastDirection;

    public EnemyType enemyType;
    private Transform player;
    public Rigidbody2D rb;
    public float health, maxHealth = 2f;
    private PlayerHealth pH;
    public Image healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FindPlayer());
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
        if (state == MoveState.Death) return;
        if (state == MoveState.Attack) return;
        ChasePlayer();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        float speed = (state == MoveState.Run) ? runSpeed : walkSpeed;
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            movement = Vector2.zero;
            state = MoveState.Walk;
            return;
        }

        Vector2 dir = (player.position - transform.position).normalized;
        movement = dir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            direction = (dir.x > 0) ? Direction.Right : Direction.Left;
        }
        else
        {
            direction = (dir.y > 0) ? Direction.Up : Direction.Down;
        }


        if (distanceToPlayer < 1.5f)
        {
            Attack();
            return;
        }
        
        if (canRun && distanceToPlayer < 3f)
        {
            state = MoveState.Run;
        }
        else
        {
            state = MoveState.Walk;
        }
    }

    private void UpdateAnimation()
    {
        //override with action animations
        if (state == MoveState.Attack || state == MoveState.Hurt)
            return;

        if (state == MoveState.Death)
        {
            anim.PlayAnimation(death, false);
            return;
        }

        if (state != lastState || direction != lastDirection)
        {
            Sprite[] newAnim = GetMovementAnimation();
            anim.PlayAnimation(newAnim, true);
            lastState = state;
            lastDirection = direction;
        }
    }

    private Sprite[] GetMovementAnimation()
    {
        if (state == MoveState.Run)
            return GetDirectionalAnim(runUp, runDown, runLeft, runRight);

        return GetDirectionalAnim(walkUp, walkDown, walkLeft, walkRight);

    }

    private Sprite[] GetDirectionalAnim(Sprite[] up, Sprite[] down, Sprite[] left, Sprite[] right)
    {
        return direction switch
        {
            Direction.Up => up,
            Direction.Down => down,
            Direction.Left => left,
            Direction.Right => right,
            _ => down
        };
    }

    public void Attack()
    {
        if (state == MoveState.Death) return;
        if (Time.time - lastAttack < attackCooldown) return; //cooldown between hits
        lastAttack = Time.time;
        state = MoveState.Attack;
        lastState = MoveState.Attack;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero; //stop movement while attacking

        Sprite[] animToPlay = GetDirectionalAnim(attackUp, attackDown, attackLeft, attackRight);

        float hitTime = (animToPlay.Length * anim.animationSpeed) / 2f; // Assuming hit occurs at mid-point of animation
        StartCoroutine(DealDamageAfter(hitTime));

        anim.PlayAnimation(animToPlay, false, () =>
        {
            state = MoveState.Walk;
            lastState = MoveState.Attack;
        });
    }

    private IEnumerator DealDamageAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pH != null && Vector2.Distance(transform.position, player.position) < 1.5f)
        {
            pH.LoseLife(damage);
        }
    }

    private IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                pH = player.GetComponent<PlayerHealth>();
            }
            yield return null;
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null) healthBar.fillAmount = health / maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (state == MoveState.Death) return;
        
        health -= damage;
        UpdateHealthUI();
        DropPowerup();

        if (health <= 0)
        {
            state = MoveState.Death;
            movement = Vector2.zero; // Stop movement on death

            QuestManager.Instance.RegisterEnemyKill(enemyType);

            anim.PlayAnimation(death, false);
            Destroy(gameObject, 1f); // Destroy after 1 second to allow death animation to play
        }
    }

    private void DropPowerup()
    {
        if (powerupPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        GameObject prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}

public enum EnemyType { Snake, Cobra, Boar }
