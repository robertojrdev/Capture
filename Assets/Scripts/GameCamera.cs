using CameraUI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class GameCamera : MonoBehaviour
{
    public static GameCamera instance;

    public event Action<Photo> OnTakePhoto_Event;

    [Header("Photos Output")]
    [SerializeField] private RectTransform sizeImageReference;

    [Header("Camera Settings")]
    [SerializeField] private float zoom = 1;
    [SerializeField] private float initialZoom = 1.5f;
    [SerializeField] private float maxZoom = 12;
    [SerializeField] private float zoomSpeed = 2;
    [SerializeField] private float maxTilt = 45;
    [SerializeField] private float tiltSpeed = 30;
    [SerializeField] private float minTimeInterval = 0.2f;
    [SerializeField] private ZoomSlider zoomSlider;
    [SerializeField] private LayerMask photoMask = -1;

    [Header("Post Process")]
    [SerializeField] private PostProcessLayer postProcessLayer;
    [SerializeField] private PostProcessVolume globalVolume;
    [SerializeField] private float maxDistFocus = 10;
    [SerializeField] private float minDistFocus = 0.1f;
    [SerializeField] private float timeToFocus = 0.7f;

    [SerializeField] private UnityEvent OnActivate;
    [SerializeField] private UnityEvent OnDeactivate;
    [SerializeField] private UnityEvent OnTakePhoto;

    public MeshRenderer ms;
    public RawImage img;
    public Camera Cam { get; private set; }
    private float OriginalFov;
    public bool IsActive { get; private set; }
    public bool IsLocked { get; private set; }
    public float Tilt { get; private set; } // tilt rotation is applied in PlayerController
    private bool isInPhotoTimeInterval = false;
    private bool isDoingZoom;
    private bool isMouseZoom = false;
    public Vector3 InitialPosiiton { get; private set; }
    private bool canTakePhoto = false;
    private bool isCallingPhoto = false;

    //post process
    DepthOfField depthOfFieldLayer = null;
    ChromaticAberration chromaticAberration = null;
    Coroutine lerpBlurRoutine;

    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
            Debug.LogError("More than a GameCamera in scene");
        }
        else
        {
            instance = this;
        }

    }
    
    private void Start()
    {
        Cam = GetComponent<Camera>();
        OriginalFov = Cam.fieldOfView;
        zoomSlider.minZoom = initialZoom;
        zoomSlider.maxZoom = maxZoom;
        zoomSlider.SetZoom(zoom);

        if (!postProcessLayer)
            postProcessLayer = GetComponent<PostProcessLayer>();

        globalVolume.profile.TryGetSettings(out depthOfFieldLayer);
        globalVolume.profile.TryGetSettings(out chromaticAberration);

        InitialPosiiton = transform.localPosition;
    }

    private void Update()
    {
        GetInputs();
    }

    private void GetInputs()
    {
        if (IsLocked)
            return;

        //Open & Close camera
        if (Input.GetButtonDown("Open Camera"))
            SetActiveCamera(true);
        if (Input.GetButtonUp("Open Camera"))
            SetActiveCamera(false);

        if (!IsActive)
            return;

        //Take a photo
        if (Input.GetButtonDown("Shot"))
            TakePhoto();

        //Do zoom
        float zoom;
        if (isMouseZoom)
            zoom = Input.GetAxis("Zoom Mouse");
        else
            zoom = Input.GetAxis("Zoom In") + Input.GetAxis("Zoom Out");
        if(canTakePhoto)
            DoZoom(zoom);

        //Do tilt
        float tilt = Input.GetAxis("Tilt Left") + Input.GetAxis("Tilt Right");
        DoTilt(tilt);
    }

    private void OnGUI()
    {
        Event evt = Event.current;
        if (evt != null && evt.isScrollWheel)
        {
            isMouseZoom = true;
        }
        else
            isMouseZoom = false;
    }

    public static void SetActiveCamera(bool active = true, bool lockToOpenInGame = false)
    {
        if (!instance || (Album.instance && Album.instance.IsVisible))
            return;

        if (active)
        {
            instance.IsActive = true;
            instance.IsLocked = false;
            

            instance.OnActivate.Invoke();
        }
        else
        {
            instance.IsLocked = lockToOpenInGame;
            instance.IsActive = false;

            if (instance.chromaticAberration)
                instance.chromaticAberration.active = false;

            instance.OnDeactivate.Invoke();
            instance.SetFocused(true);
        }
    }

    public void CanTakePhoto() // called on animation camera
    {
        canTakePhoto = IsActive;

        if(canTakePhoto)
        {
            SetZoom(instance.initialZoom);

            if (instance.chromaticAberration)
                instance.chromaticAberration.active = true;
            if (instance.depthOfFieldLayer)
                instance.depthOfFieldLayer.active = true;
        }
        else
        {

            //ResetTilt();
            //ResetZoom();

            if (instance.chromaticAberration)
                instance.chromaticAberration.active = false;
            if (instance.depthOfFieldLayer)
                instance.depthOfFieldLayer.active = false;
        }
    }

    public static void SetCameraLocked(bool locked)
    {
        instance.IsLocked = locked;
    }

    private void DoZoom(float value)
    {
        if (value != 0)
        {
            if (!isDoingZoom)
            {
                isDoingZoom = true;
                SetFocused(false);
            }
        }
        else
        {
            if (isDoingZoom)
            {
                isDoingZoom = false;
                SetFocused(true);
            }

            return;
        }

        float zoomIncrement = value * zoomSpeed * Time.deltaTime;
        zoom += zoomIncrement;
        zoom = Mathf.Clamp(zoom, initialZoom, maxZoom);
        Cam.fieldOfView = OriginalFov / zoom;
        zoomSlider.SetZoom(zoom);

        float cameraSpeedMultiplier = 1 - (zoom / maxZoom);
        cameraSpeedMultiplier = Mathf.Clamp(cameraSpeedMultiplier, 0.2f, 1);
        PlayerController.instance.LookSpeedMultiplier = cameraSpeedMultiplier;
    }

    public void SetZoom(float zoom)
    {
        zoom = Mathf.Clamp(zoom, initialZoom, maxZoom);
        this.zoom = zoom;
        Cam.fieldOfView = OriginalFov / this.zoom;
        zoomSlider.SetZoom(zoom);

        float cameraSpeedMultiplier = 1 - (zoom / maxZoom);
        cameraSpeedMultiplier = Mathf.Clamp(cameraSpeedMultiplier, 0.2f, 1);
        PlayerController.instance.LookSpeedMultiplier = cameraSpeedMultiplier;
    }

    public void SetZoomToInitial()
    {
        SetZoom(initialZoom);
    } //used on camera animation

    public void ResetZoom()
    {
        zoom = 1;
        Cam.fieldOfView = OriginalFov;

        PlayerController.instance.LookSpeedMultiplier = 1;
    }

    public void DoTilt(float value)
    {
        if (!IsActive)
            return;

        float tiltIncrement = value * tiltSpeed * Time.deltaTime;
        Tilt = Mathf.Clamp(Tilt, -maxTilt, maxTilt);

        //rotate all ui
        TiltWithCamera.RotateAll(Tilt);
    }

    public void ResetTilt()
    {
        Tilt = 0;
        Vector3 rotation = Cam.transform.localEulerAngles;
        rotation.z = Tilt;
        Cam.transform.localEulerAngles = rotation;

        //rotate all ui
        TiltWithCamera.RotateAll(Tilt);
    }

    public static void TakePhoto()
    {
        if (!instance)
            return;

        if (instance.IsActive && instance.canTakePhoto && !instance.isInPhotoTimeInterval)
        {
            PlayerController.instance.SetCameraLocked(true, false);

            Photo photo = Photo.TakeAPhoto(instance.Cam, instance.postProcessLayer, instance.photoMask, instance.sizeImageReference);
            Album.instance.AddPhoto(photo);

            //event used in inspector
            instance.OnTakePhoto.Invoke();

            //event used by script
            if (instance.OnTakePhoto_Event != null)
                instance.OnTakePhoto_Event(photo);

            PlayerController.instance.SetCameraLocked(false, false);

            instance.StartCoroutine(instance.TimeInterval());
        }
    }

    public void SetFocused(bool hasFocus)
    {
        float value = hasFocus ? maxDistFocus : minDistFocus; 

        if (lerpBlurRoutine != null)
            StopCoroutine(lerpBlurRoutine);
        lerpBlurRoutine = StartCoroutine(LerpBlur(value));
    }

    IEnumerator LerpBlur(float value)
    {
        if (depthOfFieldLayer)
        {
            float delta = 0;
            while (delta < 1)
            {
                depthOfFieldLayer.focusDistance.value = Mathf.Lerp(depthOfFieldLayer.focusDistance, value, delta);
                delta += Time.deltaTime / timeToFocus;
                yield return null;
            }

            depthOfFieldLayer.focusDistance.value = value;
        }
    }

    IEnumerator TimeInterval()
    {
        isInPhotoTimeInterval = true;
        yield return new WaitForSeconds(minTimeInterval);
        isInPhotoTimeInterval = false;
    }
}
