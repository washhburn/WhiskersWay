using UnityEngine;

public class AnimatedSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] sprites;
    private int frame;

    public float animationSpeed = 0.15f; // Time between frames in seconds
    private float timer;

    private bool loop;
    private bool isPlaying;

    public System.Action onAnimationComplete;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayAnimation(Sprite[] newSprites, bool loopAnimation)
    {
        if (newSprites == null || newSprites.Length == 0) return;

        sprites = newSprites;
        loop = loopAnimation;
        frame = 0;
        timer = 0f;
        isPlaying = true;

        onAnimationComplete = null;

        spriteRenderer.sprite = sprites[0];
    }

    private void Update()
    {
        if (!isPlaying || sprites == null) return;

        timer += Time.deltaTime;

        if (timer < animationSpeed) return;

        timer = 0f;
        frame++;

        if (frame >= sprites.Length)
        {
            if (loop)
            {
                frame = 0;
            }
            else
            {
                isPlaying = false;
                onAnimationComplete?.Invoke();
                return;
            }
        }

        spriteRenderer.sprite = sprites[frame];
    }
}
