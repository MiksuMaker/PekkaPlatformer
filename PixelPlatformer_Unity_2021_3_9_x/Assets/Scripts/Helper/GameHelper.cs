using UnityEngine;
public static class GameHelper
{
    //Tile unit size (16px tiles with 100px = 1 unit setup)
    public const float TileUnitSize = 0.16f;

    //Scenes in build as const vars for debugging (less error prone and easier to debug than strings in this stage)
    public const string MainMenu = "MainMenu";
    public const string WorldMap = "WorldMap";
    public const string LevelPre = "Level_"; //e.g. Level_1_1 = World 1, Level 1

    //This is also just to avoid typos when writing layer names as strings or numbers
    public static int PlayerLayer { get; private set; } = LayerMask.NameToLayer("Player");
    public static int EnemyLayer { get; private set; } = LayerMask.NameToLayer("Enemy");
    public static int NoCollisionLayer { get; private set; } = LayerMask.NameToLayer("NoCollision");
    public static int IgnoreAllLayer { get; private set; } = LayerMask.NameToLayer("IgnoreAllCollisions");

    public static string GetLevelNameString(int world, int level)
    {
        return LevelPre + world + "_" + level;
    }
}


