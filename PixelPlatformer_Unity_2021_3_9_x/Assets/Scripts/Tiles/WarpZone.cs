using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpZone : BaseTile
{
    public WarpZone WarpToZone;
    public Transform InOutPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
            player.OnWarpZone = this;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
            player.OnWarpZone = null;
    }
}
