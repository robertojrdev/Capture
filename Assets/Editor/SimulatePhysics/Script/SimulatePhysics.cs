using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SimulatePhysics : EditorWindow
{
    ScriptableObject target;
    SerializedObject so;


    //show variables
    [SerializeField]private int maxIterations = 1000;
    [SerializeField]private float timeIteration = 0.02f;
    [SerializeField]private bool inproveSelectedCollision = false;
    [SerializeField]private bool autoGenerateRigidBody = false;
    [SerializeField]private bool nonSelectedAsStatic = true;
    [SerializeField]private bool simulateKinematic = true;

    //static variables
    private static int maxIterations_static;

    [SerializeField] private GameObject[] selectedObjects;


    [MenuItem("Tools/Simulate Physics")]
    private static void ShowWindow()
    {
        GetWindow<SimulatePhysics>("Simulate Physics");
    }

    private void OnGUI()
    {
        target = this;
        so = new SerializedObject(target);

        GUILayout.Label("Select the objects to simulate and then press simulate");

        GUILayout.Space(10);

        maxIterations = EditorGUILayout.IntField("Max Iterations", maxIterations);
        timeIteration = EditorGUILayout.Slider("Iteration Interval Time", timeIteration, 0.005f, 0.03f);
        inproveSelectedCollision = EditorGUILayout.Toggle("Inprove Selected Collision", inproveSelectedCollision);
        autoGenerateRigidBody = EditorGUILayout.Toggle("Simulate without rigidbody", autoGenerateRigidBody);
        simulateKinematic = EditorGUILayout.Toggle("Simulate selected kinematic Rigidbody", simulateKinematic);
        nonSelectedAsStatic = EditorGUILayout.Toggle("Non selected Rigidbody as static", nonSelectedAsStatic);

        GUILayout.Space(10);

        if (GUILayout.Button("Simulate"))
            Simulate();

        GUILayout.Space(10);

        SerializedProperty objProperty = so.FindProperty("selectedObjects");
        EditorGUILayout.PropertyField(objProperty, true);



        so.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    private void SceneGUI(SceneView sceneView)
    {
        // This will have scene events including mouse down on scenes objects
        Event cur = Event.current;

        if (cur.type == EventType.MouseUp)
        {
            selectedObjects = Selection.gameObjects;
            this.Repaint();
        }
    }

    private void Simulate()
    {
        float timeCounter = Time.realtimeSinceStartup;
       
        List<GameObject> objWithoutRb;
        List<Rigidbody> selectedRb = GetSelectionRigidBody(out objWithoutRb); //get all selected objects rigidbodyes, and add RB to those ones who didn't have one (if checked) return a list of obj without rb

        //if simulate selected kinematics
        List<Rigidbody> kinematicSelectedRb = new List<Rigidbody>();
        if (simulateKinematic)
        {
            kinematicSelectedRb = selectedRb.FindAll(rb => rb.isKinematic == true);
            foreach (Rigidbody rb in kinematicSelectedRb)
            {
                rb.isKinematic = false;
            }
        }

        List<Rigidbody> nonSelectedRb = new List<Rigidbody>(FindObjectsOfType<Rigidbody>()); //get all rigidbody
        ////save state to undo before continue nonSelectedRb operation
        //List<Transform> allObjects = nonSelectedRb.Select(rb => rb.transform).ToList();
        //Debug.Log("transforms = " + allObjects.Count);
        //Undo.RecordObjects(nonSelectedRb.ToArray(), "Simulation");
        //Undo.FlushUndoRecordObjects();
        ////continue nonSelectedRb operation
        nonSelectedRb = SubtractRigidbodyLists(nonSelectedRb, selectedRb); //remove selected from all
        List <RigidbodyState> othersState = SaveRigidbodyState(nonSelectedRb); //save others state

        if (nonSelectedAsStatic) SetKinematic(nonSelectedRb); //set non selected rigid body as stitic

        //improve collision detection
        List<RigidbodyState> rbStates = new List<RigidbodyState>();
        if (inproveSelectedCollision) ImproveRigidbodyCollision(selectedRb, out rbStates);

        Physics.autoSimulation = false;

        int iterationCount = 0;
        for (int i = 0; i < maxIterations; i++)
        {
            iterationCount++;
            Physics.Simulate(timeIteration);
            if (selectedRb.All(rb => rb.IsSleeping())) //if all elements match a condition (LINK)
            {
                break;
            }
        }

        //reset selected kinematics
        foreach (Rigidbody rb in kinematicSelectedRb)
        {
            rb.isKinematic = true;
        }

        //reset inproved rbs
        if (inproveSelectedCollision)
            ResetRigidbody(selectedRb, rbStates);

        //back non selected objects to original
        ResetRigidbody(nonSelectedRb, othersState, true);

        //remove rigid bodys created
        RemoveRigidBody(objWithoutRb);

        Physics.autoSimulation = true;

        //final log
        timeCounter = Time.realtimeSinceStartup - timeCounter;
        Debug.Log("Simulation done. \nTotal iterations " + iterationCount + " - Total time " + timeCounter);
    }

    #region simulation functions
    private List<Rigidbody> GetSelectionRigidBody(out List<GameObject> objWithoutRb)
    {
        objWithoutRb = new List<GameObject>();
        List<Rigidbody> rb = new List<Rigidbody>();
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
             Rigidbody r = Selection.gameObjects[i].GetComponent<Rigidbody>();
            if (r)
            {
                rb.Add(r);
            }
            else if (autoGenerateRigidBody)
            {
                rb.Add(Selection.gameObjects[i].AddComponent<Rigidbody>());
                objWithoutRb.Add(Selection.gameObjects[i]);
            }
        }
        
        return rb;
    }

    private List<Rigidbody> SubtractRigidbodyLists(List<Rigidbody> a , List<Rigidbody> b)
    {
        for (int i = 0; i < b.Count; i++)
        {
            if (a.Contains(b[i]))
            {
                a.Remove(b[i]);
            }
        }

        return a;
    }

    private void SetKinematic(List<Rigidbody> rigidbody)
    {
        for (int i = 0; i < rigidbody.Count; i++)
        {
            //Debug.Log(rigidbody[i].name + " is Kinematic " + rigidbody[i].isKinematic);
            rigidbody[i].isKinematic = true;
            //Debug.Log(rigidbody[i].name + " is changed Kinematic " + rigidbody[i].isKinematic);
        }
    }

    private void ImproveRigidbodyCollision(List<Rigidbody> rb, out List<RigidbodyState> rbstt)
    {
        rbstt = new List<RigidbodyState>();
        for (int i = 0; i < rb.Count; i++)
        {
            rbstt.Add(new RigidbodyState(rb[i]));
            rb[i].collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    private void ResetRigidbody(List<Rigidbody> rb, List<RigidbodyState> rbstt, bool resetPosition = false)
    {
        if (rb.Count != rbstt.Count)
        {
            Debug.LogWarning("diferent sizes in reset rigid body - " + rb.Count + " - " +rbstt.Count);
            return;
        }

        for (int i = 0; i < rb.Count; i++)
        {
            RigidbodyState.SetStateToRigidbody(rbstt[i], rb[i], resetPosition);
        }
    }

    private void RemoveRigidBody(List<GameObject> rb)
    {
        for (int i = 0; i < rb.Count; i++)
        {
            DestroyImmediate(rb[i].GetComponent<Rigidbody>());
        }
    }

    private List<RigidbodyState> SaveRigidbodyState(List<Rigidbody> rb)
    {
        List<RigidbodyState> state = new List<RigidbodyState>();
        for (int i = 0; i < rb.Count; i++)
        {
            state.Add(new RigidbodyState(rb[i]));
        }
        return state;
    }

    #endregion

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneGUI;
    }

    class RigidbodyState
    {
        protected float mass;
        protected float drag;
        protected float angularDrag;
        protected bool useGravity;
        protected bool isKinematic;
        protected RigidbodyInterpolation interpolation;
        protected CollisionDetectionMode collisionDetectionMode;
        protected Vector3 position;
        protected Vector3 velocity;
        protected Quaternion rotation;

        public RigidbodyState(Rigidbody rb)
        {
            mass = rb.mass;
            drag = rb.drag;
            angularDrag = rb.angularDrag;
            useGravity = rb.useGravity;
            isKinematic = rb.isKinematic;
            interpolation = rb.interpolation;
            collisionDetectionMode = rb.collisionDetectionMode;
            position = rb.position;
            rotation = rb.rotation;
            velocity = rb.velocity;
        }

        public static Rigidbody SetStateToRigidbody(RigidbodyState rigidbodyState, Rigidbody rigidbody, bool setPosition = true, bool setVelecityZero = true)
        {
            rigidbody.mass = rigidbodyState.mass;
            rigidbody.drag = rigidbodyState.drag;
            rigidbody.angularDrag = rigidbodyState.angularDrag;
            rigidbody.useGravity = rigidbodyState.useGravity;
            rigidbody.isKinematic = rigidbodyState.isKinematic;
            rigidbody.interpolation = rigidbodyState.interpolation;
            rigidbody.collisionDetectionMode = rigidbodyState.collisionDetectionMode;
            if (setPosition)
            {
                rigidbody.transform.position = rigidbodyState.position;
                rigidbody.transform.rotation = rigidbodyState.rotation;
            }
            if (setVelecityZero)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            return rigidbody;
        }

        //public override static Rigidbody operator + (Rigidbody rb, RigidbodyState stt)
        //{
        //    rb.mass = rb.mass;
        //    rb.drag = stt.rigidBody.drag;
        //    rb.angularDrag = stt.rigidBody.angularDrag;
        //    rb.useGravity = stt.rigidBody.useGravity;
        //    rb.isKinematic = stt.rigidBody.isKinematic;
        //    rb.interpolation = stt.rigidBody.interpolation;
        //    rb.collisionDetectionMode = stt.rigidBody.collisionDetectionMode;

        //    return rb;
        //}
    }

}