using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float Speed;

    void Update()
    {
        transform.Translate(transform.forward * Time.deltaTime * Speed, Space.Self);
    }
}
