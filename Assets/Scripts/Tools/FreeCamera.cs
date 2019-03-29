using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public static FreeCamera instance;

    public static bool IsActive { get; private set; }

    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    public bool limitZoom = false;
    public float zoom = 1;
    public float maxZoom = 8;
    public float zoomSpeed = 2;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    private Camera cam;
    private float originalFov;
    public static bool IsFlying { get; private set; }

    private void Awake()
    {
        if(instance && instance != this)
        {
            instance = this;
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        gameObject.SetActive(false);
        IsActive = false;
        cam = GetComponent<Camera>();
        originalFov = cam.fieldOfView;
    }

    void Update()
    {
        if (!IsActive)
            return;

        if (Input.GetKeyDown(KeyCode.C) && !PathManager.IsActive)
        {
            PathManager.SetActive(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            IsFlying = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            IsFlying = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!IsFlying)
            return;

        rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        //Debug.Log("1: " + Quaternion.AngleAxis(rotationX, Vector3.up));
        //Debug.Log("2: " + Quaternion.AngleAxis(rotationY, Vector3.left));
        //Debug.Log("3: " + Quaternion.AngleAxis(rotationX, Vector3.up) * Quaternion.AngleAxis(rotationY, Vector3.left));

        // rotationX == transform.localEulerAngles.y
        // rotationY == ?

        //float angle = AngleOffAroundAxis(transform.forward, transform.right, transform.up);
        List<float> angles = new List<float>();

        Vector3[] vectors = { transform.forward, transform.right, transform.up, -transform.forward, -transform.right, -transform.up, Vector3.up, Vector3.right, Vector3.forward };

        Vector3 v;
        Vector3 forw;
        Vector3 axis;

        //v
        for (int i = 0; i < vectors.Length; i++)
        {
            v = vectors[i];
            //forw
            for (int j = 0; j < vectors.Length; j++)
            {
                forw = vectors[j];
                //axis
                for (int k = 0; k < vectors.Length; k++)
                {
                    axis = vectors[k];

                    angles.Add(AngleOffAroundAxis(v, forw, axis));
                }
            }
        }

        angles[0] = AngleOffAroundAxis(transform.forward, transform.forward, transform.forward);

        //Debug.Log("x: " + rotationX);
        Debug.Log("lrot: " + transform.localEulerAngles.x);
        //Debug.Log("rot: " + transform.eulerAngles.x);
        //Debug.Log("lquat: " + transform.localRotation.x);
        //Debug.Log("quat: " + transform.rotation.x);
        Debug.Log("y: " + rotationY);
        //Debug.Log("ay: " + angle * vect.y);
        //Debug.Log("ax: " + angle * vect.x);

        float dif = float.MaxValue;
        int ind = -1;
        for (int i = 0; i < angles.Count; i++)
        {
            if (angles[i] < 0)
                angles[i] *= -1;

            float ndif = rotationY  - angles[i];
            if(ndif < dif)
            {
                dif = ndif;
                ind = i;
            }
        }

        Debug.Log(ind + " Angle: " + angles[ind] + " | rot: " + rotationY);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }


        if (Input.GetKey(KeyCode.E)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.Q)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

        if (Input.GetButtonDown("Zoom In")) { DoZoom(true); }
        if (Input.GetButtonDown("Zoom Out")) { DoZoom(false); }
        if (Input.GetKey(KeyCode.KeypadEnter)) { ResetZoom(); }

        if (Input.GetKeyDown(KeyCode.End))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis)
    {
        Vector3 right = Vector3.Cross(axis, forward).normalized;
        forward = Vector3.Cross(right, axis).normalized;
        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
    }

    private void DoZoom(bool zoomIn)
    {
        float zoomIncrement = zoomSpeed * Time.deltaTime;
        zoom += zoomIn ? zoomIncrement : -zoomIncrement;
        if (limitZoom)
            zoom = Mathf.Clamp(zoom, 1, maxZoom);
        else if (zoom < 0)
            zoom = 0;
        cam.fieldOfView = originalFov / zoom;
    }

    private void ResetZoom()
    {
        zoom = 1;
        cam.fieldOfView = originalFov;
    }

    public static void Activate(bool activate, Transform reference)
    {
        if (instance)
        {
            instance.gameObject.SetActive(activate);
            IsActive = activate;
            instance.transform.position = reference.transform.position;
            instance.transform.rotation = reference.transform.rotation;

            if(!activate)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
