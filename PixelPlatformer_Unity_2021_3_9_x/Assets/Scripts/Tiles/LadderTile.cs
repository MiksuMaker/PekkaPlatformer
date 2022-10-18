using UnityEngine;

public class LadderTile : BaseTile
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            Vector3 playerCenterPos = player.transform.position;
            playerCenterPos.y += player.collider2d.size.y * .5f; //<-- get center of collider
            player.OnLadder = CheckPlayerWithinBounds(playerCenterPos) ? this : null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)        
            player.OnLadder = null;        
    }
    //Check that player's collider's center is in LadderTile's collider's bounds (especially the y-axis, for not grabbing the ladder again)
    public bool CheckPlayerWithinBounds(Vector3 playerPosition)
    {
        return collider2d.bounds.Contains(playerPosition) ? true : false;
        
    }
}
