using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform Target;

    void Update()
    {
        transform.LookAt(Target);
    }
}
