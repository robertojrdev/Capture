using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Receiver))]
public class Receiver_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Hide Debug Lines"))
        {
            Receiver.showDebugLines = !Receiver.showDebugLines;
            SceneView.RepaintAll();
        }
    }


}
