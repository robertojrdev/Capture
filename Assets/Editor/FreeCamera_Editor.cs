using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FreeCamera))]
public class FreeCamera_Editor : Editor 
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Activate"))
        {
            FreeCamera.Activate(!FreeCamera.IsActive, FreeCamera.instance.transform);
        }

    }

}
