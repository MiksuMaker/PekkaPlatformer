using UnityEngine;

public class EndTile : BaseTile
{
    public enum EndTileType { Crown, Baddie };
    public EndTileType Type;

    public Sprite CrownSprite;
    public Sprite BaddieSprite;

    private bool collected = false;

    private void OnValidate()
    {
        //End tile can be switched to use baddie sprite instead of crown sprite...
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
        if (Type == EndTileType.Crown)        
            renderer.sprite = CrownSprite;
        else
            renderer.sprite = BaddieSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController fplayer = collision.gameObject.GetComponent<PlayerController>();
        if (fplayer != null && !collected)
        {
            collected = true;

            if (Type == EndTileType.Crown)
                anim.SetTrigger("FromCrown");
            else
                anim.SetTrigger("FromBaddie");

            //Unsubscribe input, walk off screen
            fplayer.LevelCompleted();
        }
    }
}
