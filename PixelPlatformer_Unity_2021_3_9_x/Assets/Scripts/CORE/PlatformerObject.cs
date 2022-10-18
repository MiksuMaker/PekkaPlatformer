using UnityEngine;

public class PlatformerObject : MonoBehaviour
{
    protected Vector2 initPosition;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        //get SpriteRenderer component
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        //Get init values
        initPosition = transform.position;
    }

    public virtual void Reset()
    {
        //Stop all running coroutines
        StopAllCoroutines();

        //Restore init values
        transform.position = initPosition;
        
        //Activate GO
        gameObject.SetActive(true);
    }

    /**
     * Check if character jumps on Platformer object or enemy and "stomps" on it.
     * @param _stomperWorldPosition Position of character that is stomping the object.
     * @param _stompedObjectWorldPosition Position of stomped object.
     */
    public virtual bool Stomped(Vector3 _stomperWorldPosition, Vector3 _stompedObjectWorldPosition)
    {
        //Direction towards enemy as unit vector:
        Vector3 dirVect = (_stompedObjectWorldPosition - _stomperWorldPosition).normalized;

        float aboveTreshold = .25f; //<- 22.5 deg angle above the enemy

        //TODO: this is extremely simple test to see if player is coming from above. 
        //Yet sometimes it fails for no apparent reason because dirVect.y value is 
        //something very low even though it's impossible and visually does NOT look like the reported value...
        //Apparently this happens very RARELY. Needs testing.        
        return dirVect.y < -aboveTreshold;
    }
}
