using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private PlayerHealth pH;
    public AttackHitbox attackHitbox;
    public Transform aim;

    [Header("Movement")]
    private Vector2 movement;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpDistance = 2f;
    public float arcHeight = 1f;

    [Header("Animation")]
    public AnimatedSpriteRenderer anim;

    [Header("Idle Animations")]
    public Sprite[] idleUp;
    public Sprite[] idleDown;
    public Sprite[] idleLeft;
    public Sprite[] idleRight;

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

    [Header("Jump Animations")]
    public Sprite[] jumpUp;
    public Sprite[] jumpDown;
    public Sprite[] jumpLeft;
    public Sprite[] jumpRight;

    private enum MoveState { Idle, Walk, Run, Attack, Hurt, Jump, Death }
    private enum Direction { Up, Down, Left, Right }

    private MoveState state;
    private MoveState lastState;
    private Direction direction = Direction.Down; //default facing down
    private Direction lastDirection = Direction.Down;

    private bool canJump = true;
    private bool isJumping;
    private int playerLayer;
    private int jumpableLayer;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody2D>();
        pH = GetComponent<PlayerHealth>();
        playerLayer = LayerMask.NameToLayer("Player");
        jumpableLayer = LayerMask.NameToLayer("JumpableObjects");
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (state == MoveState.Death) return;
        if (state == MoveState.Jump) return;
        float speed = (state == MoveState.Run) ? runSpeed : walkSpeed; //no movement if attacking or hurt
        if (state ==MoveState.Attack || state == MoveState.Hurt) speed = 0; //override speed to 0 for attack/hurt states
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime); //move based on input and state
    }

    private void HandleInput()
    {
        movement = Vector2.zero;

        if (state == MoveState.Attack ||
            state == MoveState.Hurt ||
            state == MoveState.Jump ||
            state == MoveState.Death)
        {
            return;
        }

        //attacking
        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack();
            return;
        }

        //jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            return;
        }

        float x = 0f;
        float y = 0f;

        //direction input
        if (Input.GetKey(KeyCode.W)) y += 1;
        if (Input.GetKey(KeyCode.S)) y -= 1;
        if (Input.GetKey(KeyCode.A)) x -= 1;
        if (Input.GetKey(KeyCode.D)) x += 1;

        movement = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

        //update direction based on input
        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                direction = movement.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                direction = movement.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        bool isMoving = movement != Vector2.zero;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        //running
        if (isRunning && pH.TryUseStamina(15f * Time.deltaTime)) state = MoveState.Run;
        else if (isMoving) state = MoveState.Walk;
        else state = MoveState.Idle;

        if (movement != Vector2.zero && aim != null)
        {
            Vector3 aimDir = Vector3.left * movement.x + Vector3.down * movement.y;
            aim.rotation = Quaternion.LookRotation(Vector3.forward, aimDir);
            aim.localPosition = new Vector3(movement.x, movement.y, 0f) * 0.5f; 
        }
    }

    private void UpdateAnimation()
    {
        //override with action animations
        if (state == MoveState.Attack || state == MoveState.Jump || state == MoveState.Hurt)
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

        if (state == MoveState.Walk)
            return GetDirectionalAnim(walkUp, walkDown, walkLeft, walkRight);

        return GetDirectionalAnim(idleUp, idleDown, idleLeft, idleRight);
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

    //Action methods

    public void Attack()
    {
        if (state == MoveState.Death) return;

        state = MoveState.Attack;
        lastState = MoveState.Attack;
        rb.linearVelocity = Vector2.zero; //stop movement while attacking
        attackHitbox.Enable(); //enable hitbox to damage enemies

        Sprite[] animToPlay = GetDirectionalAnim(attackUp, attackDown, attackLeft, attackRight);
        anim.PlayAnimation (animToPlay, false, () =>
        {
            attackHitbox.Disable(); //disable hitbox after animation
            state = MoveState.Idle;
            lastState = MoveState.Attack;
        });
    }

    public void Jump()
    {
        if (!canJump || isJumping) return;
        if (state != MoveState.Idle && 
            state != MoveState.Walk && 
            state != MoveState.Run) return;

        isJumping = true;
        canJump = false;
        state = MoveState.Jump;
        lastState = MoveState.Jump;
        rb.linearVelocity = Vector2.zero;

        Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, true);

        Vector2 jumpDir = direction switch 
        { 
            Direction.Up => Vector2.up, 
            Direction.Down => Vector2.down, 
            Direction.Left => Vector2.left, 
            Direction.Right => Vector2.right,
            _ => Vector2.down 
        };

        Sprite[] animToPlay = GetDirectionalAnim(jumpUp, jumpDown, jumpLeft, jumpRight);
        float duration = animToPlay.Length * anim.animationSpeed;

        StartCoroutine(JumpMove(jumpDir, duration));

        anim.PlayAnimation(animToPlay, false, () =>
        {
            Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, false);
            movement = Vector2.zero;
            isJumping = false;
            canJump = true;
            state = MoveState.Idle;
            lastState = MoveState.Jump;
            lastDirection = direction;
        });
    }

    private IEnumerator JumpMove(Vector2 jumpDir, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / duration;

            float horizontalSpeed = Mathf.Lerp(jumpDistance / duration, 0f, t);
            Vector2 horizontal = jumpDir * horizontalSpeed * Time.fixedDeltaTime;

            float previousArc = Mathf.Sin(Mathf.PI * ((elapsed - Time.fixedDeltaTime) / duration));
            float currentArc = Mathf.Sin(Mathf.PI * t);
            float verticalDelta = (currentArc - previousArc) * arcHeight;
            Vector2 vertical = Vector2.up * verticalDelta;

            rb.MovePosition(rb.position + horizontal + vertical);
            yield return new WaitForFixedUpdate();
        }
    }

    public void Hurt()
    {
        if (state == MoveState.Hurt || state == MoveState.Death) return;
        state = MoveState.Hurt;
        rb.linearVelocity = Vector2.zero;
        movement = Vector2.zero;
        Invoke(nameof(EndHurt), 0.5f);
    }

    private void EndHurt()
    {
        if (state != MoveState.Death) state = MoveState.Idle;
    }

    public void Die()
    {
        if (state == MoveState.Death) return;

        state = MoveState.Death;
        rb.linearVelocity = Vector2.zero;
        movement = Vector2.zero;
        anim.PlayAnimation(death, false);
    }

    public void Revive()
    {
        state = MoveState.Idle;
        lastState = MoveState.Idle;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        anim.PlayAnimation(idleDown, true);
    }

    public void StartFishing (System.Action onComplete)
    {
        if (state == MoveState.Death || state == MoveState.Attack || state == MoveState.Jump) return;

        state = MoveState.Attack; //use attack state for fishing to block movement and other actions
        lastState = MoveState.Attack;
        rb.linearVelocity = Vector2.zero;

        Sprite[] animToPlay = GetDirectionalAnim(attackUp, attackDown, attackLeft, attackRight);
        anim.PlayAnimation(animToPlay, false, () =>
        {
            state = MoveState.Idle;
            lastState = MoveState.Attack;
            onComplete?.Invoke();
        });
    }


}
