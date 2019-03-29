using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Perspective))]
public class Perspactive_Editor : Editor
{

    bool isPlacingTarget = false;
    bool isPlacingLook = false;

    public void OnSceneGUI()
    {

        if (isPlacingTarget | isPlacingLook)
        {
            Event evt = Event.current;
            Perspective p = (Perspective)target;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (evt.button == 0 && (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)) //evt.button 0 is left mouse button
            {


                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (isPlacingLook)
                    {
                        p.lookPosition = hit.point + Vector3.up * 2 - p.transform.position;
                    }
                    else if (isPlacingTarget)
                    {
                        p.targetPosition = hit.point - p.transform.position;
                    }
                }

                HandleUtility.Repaint();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Place Target"))
        {
            isPlacingTarget = true;
            isPlacingLook = false;
        }

        if (GUILayout.Button("Place Look"))
        {
            isPlacingLook = true;
            isPlacingTarget = false;
        }

        if (GUILayout.Button("Done"))
        {
            isPlacingLook = false;
            isPlacingTarget = false;
        }

    }
}
