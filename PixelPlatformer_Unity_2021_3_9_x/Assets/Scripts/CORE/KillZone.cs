using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZone : MonoBehaviour
{
    private BoxCollider2D boxCollider2D;
    
    private void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CharacterBase character = collision.gameObject.GetComponent<CharacterBase>();
        if (character != null && character.DieInKillZone)
            character.Die();
    }

    private void OnDrawGizmos()
    {
        if (transform.localScale == Vector3.one)
        {
            boxCollider2D = GetComponent<BoxCollider2D>();            
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(boxCollider2D.transform.position, new Vector3(boxCollider2D.size.x, boxCollider2D.size.y, .1f));
            
        }
        else
            Debug.LogError("Scale the collider, not object!");
    }
}