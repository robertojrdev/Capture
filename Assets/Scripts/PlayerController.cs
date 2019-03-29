using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float slowSpeed = 1;
    private float runSpeed = 8;
    [SerializeField] private float lookSpeed = 5;
    private bool canRun = true;
    [SerializeField] private float maxUpDownCamAngle = 80;
    [SerializeField] private float gravityAcelerationMultiplyer = 2.5f;
    [SerializeField] private float jumpSpeed = 1;
    [Header("SFX")]
    [SerializeField] private Vector2 pitchMinMax = new Vector2(0.9f, 1.1f);
    [SerializeField] [Range(0,1)] private float walkSoundVolume = 0.3f;
    [SerializeField] private float bobSpeed = 10;
    [SerializeField] private float bobHeight = .2f;

    private bool isRunning = false;
    private bool isSlow = false;
    private CharacterController controller;
    private Vector3 walkDirection;
    private float yaw;
    bool isGrounded = false;
    private bool canMoveCamera = true;
    private bool canMovePlayer = true;

    private float verticalVelocity = 0;
    private float lookSpeedMultiplier = 1;

    private float cameraBobCounter;
    private bool cameraBobFirst;

    #region GET&SET
    public float LookSpeedMultiplier
    {
        get
        {
            return lookSpeedMultiplier;
        }
        set
        {
            lookSpeedMultiplier = value;
        }
    }
    #endregion

    private void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(this);
            Debug.LogError("Multiple PlayerControllers in the scene");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        Physics();
        Walk();
    }

    private void GetInputs()
    {
        if (Input.GetButtonDown("Interact"))
        {
            Debug.Log("InteractInput");
            GameManager.instance.Interact();
        }

        walkDirection.x = Input.GetAxis("Horizontal");
        walkDirection.z = Input.GetAxis("Vertical");

        Look(Input.GetAxis("Horizontal Camera"), Input.GetAxis("Vertical Camera"));

        if (canRun)
        {
            if (Input.GetButtonDown("Run")) isRunning = true;
            if (Input.GetButtonUp("Run")) isRunning = false;

            if (Input.GetButtonDown("Slow Down")) isSlow = true;
            if (Input.GetButtonUp("Slow Down")) isSlow = false;

        }
    }

    private void Walk()
    {
        if (canMovePlayer)
        {
            Vector3 direction = (walkDirection.z * transform.forward) + (walkDirection.x * transform.right);
            float speed = isRunning ? runSpeed : isSlow ? slowSpeed : walkSpeed;
            controller.Move(direction * speed * Time.fixedDeltaTime);

            if (direction.magnitude > 0)
                CameraBob();
        }
    }

    public void CameraBob()
    {

        Vector3 pos = GameCamera.instance.InitialPosiiton;

        float height = Mathf.Cos(cameraBobCounter);
        GameCamera.instance.Cam.transform.localPosition = pos + Vector3.up * height * bobHeight;

        float multiplyer = isRunning ? 1.5f : isSlow ? 0.7f : 1;

        cameraBobCounter += Time.deltaTime * bobSpeed * multiplyer;

        //play sound
        if (height < 0.5 && !cameraBobFirst)
        {
            SFX.PlayFootStep(SFX.FootstepType.Grass, walkSoundVolume, pitchMinMax);
            cameraBobFirst = true;
        }
        else if (height > 0.5)
            cameraBobFirst = false;
    }

    private void Jump()
    {
        if (controller.isGrounded)
        {
            controller.SimpleMove(Vector3.up * 0.01f);
            verticalVelocity = jumpSpeed;
        }
    }

    private void Look(float horizontal, float vertical)
    {
        if (canMoveCamera)
        {
            //character rotation
            Vector3 characterRotation = transform.eulerAngles;
            //characterRotation.y += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
            characterRotation.y += horizontal * (lookSpeed * lookSpeedMultiplier) * Time.deltaTime;
            transform.eulerAngles = characterRotation;

            //camera rotation
            //yaw -= Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
            yaw -= vertical * (lookSpeed * lookSpeedMultiplier) * Time.deltaTime;
            yaw = Mathf.Clamp(yaw, -maxUpDownCamAngle, maxUpDownCamAngle);
            Vector3 cameraRotation = GameCamera.instance.Cam.transform.localEulerAngles;
            cameraRotation.x = yaw;
            cameraRotation.z = GameCamera.instance.Tilt;
            GameCamera.instance.Cam.transform.localEulerAngles = cameraRotation;
        }
    }

    private void Physics()
    {
        //apply gravity to check IsGrounded
        controller.Move(Vector3.up * verticalVelocity * Time.fixedDeltaTime);

        if (!controller.isGrounded)
        {
            isGrounded = false;
            verticalVelocity += UnityEngine.Physics.gravity.y * gravityAcelerationMultiplyer * Time.fixedDeltaTime; //gravity is a negative value
            verticalVelocity = Mathf.Clamp(verticalVelocity, -14, 14); //set max vertical velocity to 50 km/h
        }
        else
        {
            isGrounded = true;
            verticalVelocity = -0.1f; // to keep IsGrounded
        }
    }

    #region External Functions

    public void SetCameraLocked(bool locked, bool affectCursor = true)
    {
        if (locked)
        {
            canMoveCamera = false;
            if (affectCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            canMoveCamera = true;
            if (affectCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void SetMovimentationLocked(bool locked)
    {
        canMovePlayer = !locked;
    }

    public static void SetActive(bool active)
    {
        if(instance)
        {
            instance.gameObject.SetActive(active);
        }
    }

    #endregion
}
