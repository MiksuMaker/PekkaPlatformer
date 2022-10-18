using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Breaking bricks also bounce enemies
public class BreakingBrick : BounceTile
{    
    public bool AllowAutoBreak = true;

    [HideInInspector]
    public static BreakingBrick SingleBrickToBreak;  //<-- this trick ensures that only single brick is hit at time

    private List<BrickPiece> brickPieces = new List<BrickPiece>();

    protected override void Start()
    {
        base.Start();

        brickPieces = GetComponentsInChildren<BrickPiece>().ToList();
        //Disable all pieces
        brickPieces.ForEach(b => b.gameObject.SetActive(false));        
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (playerCollidesFromBelow != null && AllowAutoBreak)
        {            
            SingleBrickToBreak = this;
            SingleBrickToBreak.Break(playerCollidesFromBelow);
        }
    }
    
    public void Break(PlayerCharacter player)
    {
        //NOTE: only to play break sound, this is not good approach for sounds...
        playerCollidesFromBelow.BreakTile();

        brickPieces.ForEach(b => {
            //Unparent from this trans
            b.transform.SetParent(null);
            b.gameObject.SetActive(true);
        });

        SetActiveAndEnabled(false);
    }

    public override void Reset()
    {
        base.Reset();

        //Parent back to this transform
        brickPieces.ForEach(b => {
            b.transform.SetParent(transform);
            b.Reset();
            b.gameObject.SetActive(false);
        });        
    }
}
