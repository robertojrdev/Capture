using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class BrushTool : EditorWindow
{
    //made by Roberto Gomes @Dino - roberto.rmg.rj@gmail.com

    private bool canPaint = false;
    private float brushSize = 5;
    private float intensity = 0.1f;
    private KeyCode paintKey = KeyCode.Space;

    private bool paintKeyPessed = false;
    private bool canPlace = true;

    [SerializeField] private GameObject[] assets;
    private static GameObject[] assets_static = new GameObject[3];

    [SerializeField] private GameObject projector;
    private static GameObject projector_static;
    private static GameObject projector_instance;

    private static List<GameObject> placedList = new List<GameObject>();

    private static GameObject parent;

    [MenuItem("Tools/GameObjects Brush")]
    public static void ShowWindow()
    {
        GetWindow<BrushTool>("GameObjects Brush");
    }

    [ExecuteInEditMode]
    private void OnGUI()
    {
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);

        GUILayout.Space(10);
        GUILayout.Label("Add objects to list, set active paint mode, select the\n surface you want \"paint\" and hold paint button above a surface");

        SerializedProperty projectorProp = so.FindProperty("projector");
        EditorGUILayout.PropertyField(projectorProp, true);

        GUILayout.Space(10);
        canPaint = GUILayout.Toggle(canPaint ,"Active paint mode");
        paintKey = (KeyCode) EditorGUILayout.EnumPopup("Paint button", paintKey);

        GUILayout.Space(10);
        brushSize =  EditorGUILayout.Slider("Brush Size" ,brushSize, 0, 20);
        intensity = EditorGUILayout.Slider("Intensity", intensity, 0, 1);

        GUILayout.Space(10);

        SerializedProperty objProperty = so.FindProperty("assets");
        EditorGUILayout.PropertyField(objProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        if(GUILayout.Button("Erase All"))
            EraseAllPlacedObjects();

        SaveState();
        InstantiateProjector(canPaint);
        SceneView.RepaintAll();
    }

    void SceneGUI(SceneView sceneView)
    {
        // This will have scene events including mouse down on scenes objects
        Event cur = Event.current;

        if (cur.type == EventType.KeyDown && cur.keyCode == paintKey)
        {
            paintKeyPessed = true;
        }

        if (cur.type == EventType.KeyUp && cur.keyCode == paintKey)
        {
            paintKeyPessed = false;
            Debug.Log(placedList.Count);
        }

        if (cur.type == EventType.MouseMove && EditorPrefs.GetBool("canPaint"))
        {
            Vector2 mousePos = cur.mousePosition;
            mousePos.y = Camera.current.pixelHeight - mousePos.y;
            Vector3 position = Camera.current.ScreenPointToRay(mousePos).origin;

            Ray ray = Camera.current.ScreenPointToRay(mousePos);
            RaycastHit hit;
            foreach (GameObject obj in Selection.gameObjects)
            {
                Collider col = obj.GetComponent<Collider>();
                if (col)
                {
                    if (col.Raycast(ray, out hit, 1000))
                    {
                        if (projector_instance != null)
                            projector_instance.transform.position = hit.point;

                        if (paintKeyPessed)
                        {
                            //instantiate
                            float timer = Time.realtimeSinceStartup;
                            for (int i = 0; i < assets.Length; i++)
                            {
                                if (assets[i])
                                {
                                    //generate a random point in a circle vector 3
                                    Vector3 randomInCircle = UnityEngine.Random.insideUnitCircle;
                                    randomInCircle.z = randomInCircle.y;
                                    randomInCircle.y = 0;

                                    //set size of circle
                                    randomInCircle *= brushSize;

                                    //put the circle in mouse position
                                    randomInCircle += hit.point;

                                    //set point up to raycast
                                    Vector3 rayStart = randomInCircle + Vector3.up * 10;

                                    Ray ray2 = new Ray(rayStart, Vector3.down);
                                    RaycastHit hit2;

                                    //raycast to know if is above the selected surface
                                    if (col.Raycast(ray2, out hit2, 1000))
                                    {
                                        GameObject g = Instantiate(assets[i], randomInCircle, Quaternion.identity);
                                        if (!parent)
                                            parent = new GameObject() { name = "Brush Objs" };
                                        else
                                            g.transform.parent = parent.transform;
                                        placedList.Add(g);

                                        canPlace = false;
                                    }
                                }
                            }
                        }
                        SceneView.RepaintAll();
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneGUI;

        canPaint = EditorPrefs.GetBool("canPaint");
        brushSize = EditorPrefs.GetFloat("brushSize");
        intensity = EditorPrefs.GetFloat("intensity");
        paintKey = (KeyCode) EditorPrefs.GetInt("paintKey");
        assets = assets_static;
        projector = projector_static;

        if (canPaint)
            InstantiateProjector(true);
    }

    private void OnDisable()
    {
        SaveState();
        InstantiateProjector(false);
    }

    private void SaveState()
    {
        EditorPrefs.SetBool("canPaint", canPaint);
        EditorPrefs.SetFloat("brushSize", brushSize);
        EditorPrefs.SetFloat("intensity", intensity);
        EditorPrefs.SetInt("paintKey", paintKey.GetHashCode());
        assets_static = assets;
        projector_static = projector;
    }

    private void InstantiateProjector(bool toogleState)
    {
        SetupProjector();
        if (toogleState && !projector_instance && projector)
        {
            Quaternion rot = Quaternion.Euler(90, 0, 0);
            projector_instance = Instantiate(projector, Vector3.zero, rot);
            projector_instance.hideFlags = HideFlags.HideAndDontSave;
        }
        else if (!toogleState)
        {
            if(projector_instance)
                DestroyImmediate(projector_instance);

            projector_instance = null;
        }
    }

    private void SetupProjector()
    {
        if (projector_instance)
        {
            Projector p = projector_instance.GetComponent<Projector>();
            p.orthographicSize = brushSize;
        }
    }

    private void EraseAllPlacedObjects()
    {
        for (int i = 0; i < placedList.Count; i++)
        {
            DestroyImmediate(placedList[i]);
        }
        placedList = new List<GameObject>();
    }
}

public class Hider : EditorWindow
{

    [MenuItem("Window/GameObject_Hider&Destroyer")]
    public static void Create()
    {
        GetWindow<Hider>();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Select hidden"))
        {
            Selection.activeGameObject = GameObject.Find("Projector(Clone)");
        }

        if (GUILayout.Button("Select test objects"))
        {
            Selection.objects = GameObject.FindGameObjectsWithTag("TEST");
        }

        if (GUILayout.Button("Destory Selected Object"))
        {
            DestroyImmediate(Selection.activeObject);
        }
    }
}