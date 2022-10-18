using System.Collections.Generic;
using UnityEngine;

public class BounceTile : BaseTile
{
    public float BounceUpForce;

    protected PlayerCharacter playerCollidesFromBelow = null;
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        playerCollidesFromBelow = null;
        PlayerCharacter playerRef = collision.gameObject.GetComponent<PlayerCharacter>();
        if (playerRef != null)
        {
            List<Enemy> overlappingEnemies = GetOverlappingObjectColliders<Enemy>();
            overlappingEnemies.ForEach(e =>
            {
                if (e.BounceFromBricks)
                    e.Rb2D.AddForce(new Vector2(0, BounceUpForce), ForceMode2D.Impulse);
            });

            RayHitInfo topHit = playerRef.TopCheck(true);
            playerCollidesFromBelow = (topHit != null && topHit.Hit.collider.gameObject == gameObject) ? playerRef : null;            
        } 
    }
}
