using UnityEngine;

public class CollectableBase : PlatformerObject
{
    private void OnTriggerEnter2D(Collider2D collision)
    {        
        PlayerCharacter player = collision.GetComponent<PlayerCharacter>();

        if (player != null)        
            Collect(player);        
    }

    protected virtual void Collect(PlayerCharacter collector)
    {
        gameObject.SetActive(false);
    }
}
