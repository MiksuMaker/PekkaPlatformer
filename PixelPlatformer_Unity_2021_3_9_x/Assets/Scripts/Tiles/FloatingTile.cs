using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTile : BaseTile
{
    public float FloatTileDistance = 1f;
    public float FloatSpeed = 1f;
    public float StartTimeOffset = 0f;

    public enum FloatDir { Horizontal, Vertical };
    public FloatDir FloatDirection;

    private Vector3 floatPosition;
    private float startTime;

    private float test;

    protected override void Start()
    {
        base.Start();
        startTime = Time.time;

        test = Mathf.Sin((Time.time + StartTimeOffset) * FloatSpeed) * (FloatTileDistance * 1f) * GameHelper.TileUnitSize;
        Debug.Log(test);

        StartCoroutine(FloatRoutine());
    }

    private IEnumerator FloatRoutine()
    {
        

        while (true)
        {
            floatPosition = initPosition;
            
            float sinOffset = 0f;

            //if (Time.time - startTime > StartTimeOffset)
                sinOffset = Mathf.Sin( (Time.time + StartTimeOffset) * FloatSpeed) * FloatTileDistance * GameHelper.TileUnitSize;

            if (FloatDirection == FloatDir.Horizontal)
                floatPosition.x += sinOffset - test;
            else
                floatPosition.y += sinOffset - test;

            transform.position = floatPosition;
            yield return new WaitForFixedUpdate();
        }
    }
}
