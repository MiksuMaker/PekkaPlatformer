using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


public class EditorSortingLayerAlpha : EditorWindow
{
    private static List<SortingLayer> sortingLayers = new List<SortingLayer>();
    private static Dictionary<SortingLayer, int> sliderValues = new Dictionary<SortingLayer, int>();
    [MenuItem("Tools/Sorting Layer Alpha")]
    static void Init()
    {
        sortingLayers = SortingLayer.layers.ToList();
        sortingLayers.ForEach(sl =>
        {
            if (!sliderValues.ContainsKey(sl))
                sliderValues[sl] = 255;
        });

        EditorSortingLayerAlpha window = (EditorSortingLayerAlpha)EditorWindow.GetWindow(typeof(EditorSortingLayerAlpha));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Set Sorting Layer's Sprite Alpha", EditorStyles.boldLabel);

        sortingLayers.ForEach(sLayer =>
        {
            EditorGUI.BeginChangeCheck();
            sliderValues[sLayer] = EditorGUILayout.IntSlider(sLayer.name, sliderValues[sLayer], 0, 255);
            if (EditorGUI.EndChangeCheck())
                SetSpriteRendererAlphaValues(sLayer.name, sliderValues[sLayer]);
        });

        if (GUILayout.Button("Reset All to 255"))
        {
            sortingLayers.ForEach(sLayer =>
            {
                SetSpriteRendererAlphaValues(sLayer.name, 255);
                sliderValues[sLayer] = 255;
            });            
        }
    }

    private void SetSpriteRendererAlphaValues(string sortingLayerName, int newAlpha)
    {
        FindObjectsOfType<SpriteRenderer>().ToList().
            Where(sr => sr.sortingLayerName == sortingLayerName).
            ToList().
            ForEach(spriteRenderer => spriteRenderer.
            color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha / 255f));
    }
}