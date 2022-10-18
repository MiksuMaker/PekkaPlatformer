using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPiece : BaseTile
{
    public Vector2 ForceOnAwakeDir;
    public float ForceAmount;

    private void OnEnable()
    {
        AddImpulseForce(ForceOnAwakeDir, ForceAmount);
    }
}
