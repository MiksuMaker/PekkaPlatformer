using UnityEngine;

public class CameraConstraint : MonoBehaviour
{
    public bool StartRect = false;

    private BoxCollider2D boxCollider2D;
    private void OnDrawGizmos()
    {
        if (transform.localScale == Vector3.one)
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireCube(boxCollider2D.transform.position, new Vector3(boxCollider2D.size.x, boxCollider2D.size.y, .1f));

        }
        else
            Debug.LogError("Scale the collider, not object!");
    }
}
