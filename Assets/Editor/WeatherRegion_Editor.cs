using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeatherRegion))]
public class WeatherRegion_Editor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Get sun info"))
        {
            WeatherRegion wr = (WeatherRegion)target;
            wr.color = RenderSettings.sun.color;
            wr.intensity = RenderSettings.sun.intensity;
            wr.rotation = RenderSettings.sun.transform.eulerAngles;
        }

    }
}
