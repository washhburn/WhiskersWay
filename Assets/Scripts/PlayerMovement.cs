using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Movement")]
    private Vector2 movement;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

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
    public Sprite[] attack;
    public Sprite[] hurt;
    public Sprite[] jump;
    public Sprite[] death;

    private enum MoveState { Idle, Walk, Run, Attack, Hurt, Jump, Death }
    private enum Direction { Up, Down, Left, Right }
    private MoveState state;
    private Direction direction;

    private Sprite[] currentAnim;
    private MoveState lastState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        float speed = (state == MoveState.Run) ? runSpeed : walkSpeed;

        if (state ==MoveState.Attack || state == MoveState.Hurt || state == MoveState.Jump) speed = 0;

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void HandleInput()
    {
        if (state == MoveState.Attack || 
            state == MoveState.Hurt || 
            state == MoveState.Jump ||
            state == MoveState.Death)
        {
            return;
        }

        movement = Vector2.zero;

        //direction input
        if (Input.GetKey(KeyCode.W))
        {
            movement = Vector2.up;
            direction = Direction.Up;
        }

        else if (Input.GetKey(KeyCode.S))
        {
            movement = Vector2.down;
            direction = Direction.Down;
        }

        else if (Input.GetKey(KeyCode.A))
        {
            movement = Vector2.left;
            direction = Direction.Left;
        }

        else if (Input.GetKey(KeyCode.D))
        {
            movement = Vector2.right; 
            direction = Direction.Right;
        }

        bool isMoving = movement != Vector2.zero;   
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        //running
        if (isRunning)
             state = MoveState.Run;
        else if (isMoving)
             state = MoveState.Walk;
        else
             state = MoveState.Idle;

        //attacking
        if (Input.GetKeyDown(KeyCode.LeftControl)) 
            Attack();
    }

    private void UpdateAnimation()
    {
        //override with action animations
        if (state == MoveState.Attack)
        {
            anim.PlayAnimation(attack, false);
            return;
        }
        else if (state == MoveState.Hurt)
        {
            anim.PlayAnimation(hurt, false);
            return;
        }
        else if (state == MoveState.Jump)
        {
            anim.PlayAnimation(jump, false);
            return;
        }
        else if (state == MoveState.Death)
        {
            anim.PlayAnimation(death, false);
            return;
        }

        //movement animations
        Sprite[] newAnim = GetMovementAnimation();

        if (newAnim != currentAnim || state != lastState)
        {
            currentAnim = newAnim;
            lastState = state;
            anim.PlayAnimation(currentAnim, true);
        }
    }
    
    private Sprite[] GetMovementAnimation()
    {
        if (state == MoveState.Run)
        {
            return direction switch
            {
                Direction.Up => runUp,
                Direction.Down => runDown,
                Direction.Left => runLeft,
                Direction.Right => runRight,
                _ => runDown
            };
        }
        
        if (state == MoveState.Walk)
        {
            return direction switch
            {
                Direction.Up => walkUp,
                Direction.Down => walkDown,
                Direction.Left => walkLeft,
                Direction.Right => walkRight,
                _ => walkDown
            };
        }
        
        //Idle
        return direction switch
        {
            Direction.Up => idleUp,
            Direction.Down => idleDown,
            Direction.Left => idleLeft,
            Direction.Right => idleRight,
            _ => idleDown
        };
    }

    //Action methods

    public void Attack()
    {
        if (state == MoveState.Attack || state == MoveState.Death) return;

        state = MoveState.Attack;

        anim.onAnimationComplete = () =>
        {
            state = MoveState.Idle;
        };
    }

    public void Hurt()
    {
        state = MoveState.Hurt;
        anim.onAnimationComplete = () =>
        {
            state = MoveState.Idle;
        };
    }

    public void Jump()
    {
        if (state != MoveState.Idle && 
            state != MoveState.Walk && 
            state != MoveState.Run) return;
        
        state = MoveState.Jump;
        anim.onAnimationComplete = () =>
        {
            state = MoveState.Idle;
        };
    }

    public void Die()
    {
        state = MoveState.Death;
    }

}
