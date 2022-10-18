using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public float AnimationFrameDelay = .1f;
    public float RandomStartDelay = 0f;
    public float RandomLoopDelay = 0f;

    private int currentFrame = 0;
    private int totalFrames;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        totalFrames = Sprites.Count;        
    }

    private void OnEnable()
    {
        //Start coroutine in OnEnable, so that coroutine resets for PlatformerObjects that are disabled (e.g. Coin)
        StartCoroutine(Animation());
    }

    IEnumerator Animation()
    {
        //Random delay in start
        yield return new WaitForSeconds(Random.Range(0, RandomStartDelay));

        while(true)
        {
            if (currentFrame < totalFrames - 1)
            {
                currentFrame++;
            }
            else
            {
                currentFrame = 0;
            }

            spriteRenderer.sprite = Sprites[currentFrame];

            //Random delay after every loop
            if (currentFrame == 0)
                yield return new WaitForSeconds(Random.Range(0, RandomLoopDelay));

            yield return new WaitForSeconds(AnimationFrameDelay);
        }
    }
}
