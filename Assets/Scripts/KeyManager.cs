using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class KeyManager : MonoBehaviour
{
    public static KeyManager instance;

    
    [Header("Keys")]
    [Space(10)]

    [Header("walk")]
    public Keys walkHorizontal = new Keys() {axis = new string[] {"Horizontal", "XB_L Horizontal", "XB_A Horizontal" } };
    public Keys walkVertical = new Keys() { axis = new string[] { "Vertical", "XB_L Vertical", "XB_A Vertical" } };
    public Keys controller_Run;
    public Keys controller_Jump;
    [Space(10)]
    [Header("camera")]
    public Keys cameraHorizontal = new Keys() { axis = new string[] { "Mouse X", "XB_R Horizontal" } };
    public Keys cameraVertical = new Keys() { axis = new string[] { "Mouse Y", "XB_R Vertical" } };
    public Keys cameraZoomIn = new Keys() { axis = new string[] {"XB_TR"} };
    public Keys cameraZoomOut = new Keys() { axis = new string[] {"XB_TL"} };
    public Keys camera_TiltRight;
    public Keys camera_TiltLeft;
    public Keys camera_Activate;
    public Keys camera_TakePhoto;
    [Space(10)]
    [Header("interaction")]
    public Keys axisNavigationnHorizontal = new Keys() { axis = new string[] { "Horizontal", "XB_L Horizontal" } };
    public Keys axisNavigationnVertical = new Keys() { axis = new string[] { "Vertical", "XB_L Vertical" } };
    public Keys keyNavigationUp;
    public Keys keyNavigationDown;
    public Keys keyNavigationRight;
    public Keys keyNavigationLeft;
    public Keys interact;
    public Keys reloadScene;
    public Keys album_ShowHide;
    public Keys freeCameraMode;
    public Keys pathManager;
    [Space(10)]
    [Header("Menu Interaction Buttons")]
    public Keys select; 
    public Keys back;    


    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
            Debug.LogWarning("Multiple KeyManager instances");
        }
        else
        {
            instance = this;
        }

        

        #region Initialize Keys
        walkHorizontal.InitializeKey();
        walkVertical.InitializeKey();
        controller_Run.InitializeKey();
        controller_Jump.InitializeKey();
        cameraHorizontal.InitializeKey();
        cameraVertical.InitializeKey();
        cameraZoomIn.InitializeKey();
        cameraZoomOut.InitializeKey();
        camera_TiltRight.InitializeKey();
        camera_TiltLeft.InitializeKey();
        camera_Activate.InitializeKey();
        camera_TakePhoto.InitializeKey();
        axisNavigationnHorizontal.InitializeKey();
        axisNavigationnVertical.InitializeKey();
        keyNavigationUp.InitializeKey();
        keyNavigationDown.InitializeKey();
        keyNavigationRight.InitializeKey();
        keyNavigationLeft.InitializeKey();
        interact.InitializeKey();
        reloadScene.InitializeKey();
        album_ShowHide.InitializeKey();
        freeCameraMode.InitializeKey();
        pathManager.InitializeKey();
        select.InitializeKey();
        back.InitializeKey();
        #endregion

    }

    private void Start()
    {

    }

}

[System.Serializable]
public class Keys
{
    public KeyCode[] keys = new KeyCode[2];
    public string[] axis = new string[0];
    public bool limitAxis = true;
    public bool axisAsKeys = true;
    [HideInInspector] public UnityEvent OnKeyDown;
    [HideInInspector] public UnityEvent OnKeyUp;
    [HideInInspector] public UnityEvent OnKey;
    public bool GetKeyDown { get; private set; }
    public bool GetKeyUp { get; private set; }
    public bool GetKey { get; private set; }

    public bool GetAxisPositiveDown { get; private set; }
    public bool GetAxisPositiveUp { get; private set; }
    public bool GetAxisPositive { get; private set; }
    public bool GetAxisNegativeDown { get; private set; }
    public bool GetAxisNegativeUp { get; private set; }
    public bool GetAxisNegative { get; private set; }

    public static List<Keys> AllKeys = new List<Keys>();

    public static KeyCode camera_ZoomIn = KeyCode.KeypadPlus;
    public static KeyCode camera_ZoomOut = KeyCode.KeypadMinus;
    public static KeyCode camera_TiltRight = KeyCode.E;
    public static KeyCode camera_TiltLeft = KeyCode.Q;
    public static KeyCode camera_Activate = KeyCode.Mouse1;
    public static KeyCode camera_TakePhoto = KeyCode.Mouse0;
    public static KeyCode controller_Run = KeyCode.LeftShift;
    public static KeyCode controller_Jump = KeyCode.Space;
    public static KeyCode album_ShowHide = KeyCode.Tab;
    public static KeyCode interact = KeyCode.Mouse0;
    public static KeyCode freeCameraMode = KeyCode.C;
    public static KeyCode pathManager = KeyCode.V;

    public void InitializeKey()
    {
        if(!AllKeys.Contains(this))
        {
            AllKeys.Add(this);
        }
    }

    public static void Update()
    {
        for (int i = 0; i < AllKeys.Count; i++)
        {
            #region axis as keys
            if (AllKeys[i].axisAsKeys)
            {
                float axisValue = AllKeys[i].GetAxis();

                if (axisValue > 0.5f)
                {
                    //GetAxisPositiveDown
                    if (AllKeys[i].GetAxisPositive == false)
                    {
                        AllKeys[i].GetAxisPositive = true;
                        AllKeys[i].GetAxisPositiveDown = true;
                        AllKeys[i].GetAxisPositiveUp = false;
                    }
                    //GetAxisPositive
                    else
                    {
                        AllKeys[i].GetAxisPositive = true;
                        AllKeys[i].GetAxisPositiveDown = false;
                        AllKeys[i].GetAxisPositiveUp = false;
                    }
                }

                //GetAxisPositiveUp
                else if (AllKeys[i].GetAxisPositive)
                {
                    AllKeys[i].GetAxisPositive = false;
                    AllKeys[i].GetAxisPositiveDown = false;
                    AllKeys[i].GetAxisPositiveUp = true;
                }


                //negative
                else if (axisValue < -0.5f)
                {
                    //GetAxisNegativeDown
                    if (AllKeys[i].GetAxisNegative == false)
                    {
                        AllKeys[i].GetAxisNegative = true;
                        AllKeys[i].GetAxisNegativeDown = true;
                        AllKeys[i].GetAxisNegativeUp = false;
                    }
                    //GetAxisNegative
                    else
                    {
                        AllKeys[i].GetAxisNegative = true;
                        AllKeys[i].GetAxisNegativeDown = false;
                        AllKeys[i].GetAxisNegativeUp = false;
                    }
                }
                //GetAxisNegativeUp
                else if (AllKeys[i].GetAxisNegative)
                {
                    AllKeys[i].GetAxisNegative = false;
                    AllKeys[i].GetAxisNegativeDown = false;
                    AllKeys[i].GetAxisNegativeUp = true;
                }
                else
                {
                    AllKeys[i].GetAxisPositive = false;
                    AllKeys[i].GetAxisPositiveDown = false;
                    AllKeys[i].GetAxisPositiveUp = false;
                    AllKeys[i].GetAxisNegative = false;
                    AllKeys[i].GetAxisNegativeDown = false;
                    AllKeys[i].GetAxisNegativeUp = false;
                }
            }
            #endregion

            #region keys input
            for (int j = 0; j < AllKeys[i].keys.Length; j++)
            {
                if (Input.GetKeyDown(AllKeys[i].keys[j]))
                {
                    AllKeys[i].OnKeyDown.Invoke();
                    AllKeys[i].GetKey = true;
                    AllKeys[i].GetKeyDown = true;
                    AllKeys[i].GetKeyUp = false;
                    break;
                }
                else if (Input.GetKey(AllKeys[i].keys[j]))
                {
                    AllKeys[i].OnKey.Invoke();
                    AllKeys[i].GetKey = true;
                    AllKeys[i].GetKeyDown = false;
                    AllKeys[i].GetKeyUp = false;
                    break;
                }
                else if (Input.GetKeyUp(AllKeys[i].keys[j]))
                {
                    AllKeys[i].OnKeyUp.Invoke();
                    AllKeys[i].GetKey = false;
                    AllKeys[i].GetKeyDown = false;
                    AllKeys[i].GetKeyUp = true;
                    break;
                }
                else
                {
                    AllKeys[i].GetKey = false;
                    AllKeys[i].GetKeyDown = false;
                    AllKeys[i].GetKeyUp = false;
                }
            }
            #endregion
        }
    }

    public float GetAxisRaw()
    {
        float input = 0;
        for (int i = 0; i < axis.Length; i++)
        {
            //input += Input.GetAxisRaw(axis[i]);
        }

        if (limitAxis)
            return Mathf.Clamp(input, -1, 1);
        else
            return input;
    }

    public float GetAxis()
    {
        float input = 0;
        for (int i = 0; i < axis.Length; i++)
        {
            input += Input.GetAxis(axis[i]);
        }

        if (limitAxis)
            return Mathf.Clamp(input, -1, 1);
        else
            return input;
    }
}
