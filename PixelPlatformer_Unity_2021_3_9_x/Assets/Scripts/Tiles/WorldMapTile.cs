using UnityEngine;
using UnityEditor;

public class WorldMapTile : MonoBehaviour
{
    public enum TileType { Road, Level, Special }    
    public TileType Type;
        
    [HideInInspector]
    public int SelectedWorld;
    [HideInInspector]
    public int SelectedLevel; 
}

#if UNITY_EDITOR

//Custom editor for WorldMapTile
[CustomEditor(typeof(WorldMapTile))]
public class WorldMapTileEditor : Editor
{    
    string[] worldNames = new string[] { "World 1", "World 2", "World 3", "World 4", "World 5", "World 6" };
    string[] levelNames = new string[] { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6" };
    int[] values = { 1, 2, 3, 4, 5, 6 };
    
    SerializedProperty m_SelectedWorld;
    SerializedProperty m_SelectedLevel;

    protected virtual void OnEnable()
    {
        m_SelectedWorld = this.serializedObject.FindProperty("SelectedWorld");
        m_SelectedLevel = this.serializedObject.FindProperty("SelectedLevel");
    }

    //Override editor GUI for WorldMapTile to show world & map selections only if tile is type of Level
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        this.serializedObject.Update();

        base.OnInspectorGUI();     

        //Show world and level selection as integer popup with set names & values

        WorldMapTile mapTile = target as WorldMapTile;
                
        if (mapTile.Type == WorldMapTile.TileType.Level)
        {
            m_SelectedWorld.intValue = EditorGUILayout.IntPopup("Select World: ", m_SelectedWorld.intValue, worldNames, values);
            m_SelectedLevel.intValue = EditorGUILayout.IntPopup("Select Level: ", m_SelectedLevel.intValue, levelNames, values);
        }

        this.serializedObject.ApplyModifiedProperties();
    }    
}

#endif