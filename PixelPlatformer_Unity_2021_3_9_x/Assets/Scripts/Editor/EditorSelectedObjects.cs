using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class EditorSelectedObjects : EditorWindow
{
    private static string[] selectionGUIDs;

    [MenuItem("Tools/Project Selection")]
    private static void Init()
    {
        EditorSelectedObjects window = (EditorSelectedObjects)GetWindow(typeof(EditorSelectedObjects));
        window.titleContent = new GUIContent("Project Selection");        
        window.Show();

        selectionGUIDs = Selection.assetGUIDs;
    }

    void OnGUI()
    {
        if (selectionGUIDs != null && selectionGUIDs.Length > 0)
        {
            GUILayout.Label("Selected Objects Count: " + selectionGUIDs.Length, EditorStyles.boldLabel);
            if (GUILayout.Button("Save Selection"))
                SaveSelection();
        }
        else
            EditorGUILayout.HelpBox("No project objects selected. Select some objects to save the selection.", MessageType.Warning);

        if (GUILayout.Button("Load Selection"))
            LoadLastSavedSelection();
    }

    void OnSelectionChange()
    {
        selectionGUIDs = Selection.assetGUIDs;
        Repaint();
    }

    private void SaveSelection()
    {
        if (selectionGUIDs == null || selectionGUIDs.Length == 0)
            return;

        var saveStr = "";
        selectionGUIDs.ToList().ForEach(id => saveStr += id.ToString() + ";");        
        saveStr = saveStr.TrimEnd(char.Parse(";"));

        EditorPrefs.SetString("SelectedGUIDs", saveStr);
    }

    private void LoadLastSavedSelection()
    {
        List<string> guids = EditorPrefs.GetString("SelectedGUIDs").Split(char.Parse(";")).ToList();

        List<UnityEngine.Object> selectionObjects = new List<UnityEngine.Object>();
        guids.ForEach(guid => {
             selectionObjects.Add(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)));
        });

        Selection.objects = selectionObjects.ToArray();
    }
}