using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Commons;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 250f;
    [SerializeField] private float mouseXSensitivity = 500f;
    [SerializeField] private float mouseYSensitivity = 500f;
    [SerializeField] private int minXRotation = -60;
    [SerializeField] private int maxXRotation = +60;
    private float pitch = 0;
    private float yaw = 0;

    [Header("Interact")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float grabberDistance = 1.5f;
    [SerializeField] private float grabberYOffset = -0.11f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float grabberPositionSmoothSpeed = 55;
    [SerializeField] private float grabberRotationSmoothSpeed = 50;
    [SerializeField] private Slider throwSlider;
    public GameObject pickedItem = null;
    public event Action<InteractInformation> Interacted;
    public event Action<ThrowInformation> Thrown;
    private float throwTime = 0f;
    private float maxThrowTime = 2f;
    private Vector3 smoothedGrabberPosition;
    private Quaternion smoothedGrabberRotation;
    private bool isInteracting = false;
    private bool isThrowing = false;

    [Header("Misc")]
    [SerializeField] private Transform startingPoint;
    [SerializeField] private float cameraYOffset = 0.8f;
    private Rigidbody rbPlayer;
    private Transform tPlayer;
    private Transform tCameraDirection;
    private Transform tGrabber;
    private Transform tRenderer;
    

    void Start()
    {
        // Get References
        rbPlayer = GetComponent<Rigidbody>();
        tPlayer = GetComponent<Transform>();
        tCameraDirection = tPlayer.Find("CameraDirection");
        tGrabber = tPlayer.Find("Grabber");
        tRenderer = tPlayer.Find("Renderer&Collider");
        if (throwSlider == null) { Debug.LogError("Throw Slider not assigned!"); }

        // Apply components properties
        rbPlayer.constraints = RigidbodyConstraints.FreezeRotation;
        smoothedGrabberPosition = tGrabber.position;
        smoothedGrabberRotation = tGrabber.rotation;

        // Spawn on Starting point TODO: not really working as intended. probably best to move to a game manager
        if (startingPoint != null )
        {
            Cursor.lockState = CursorLockMode.None;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Cursor.SetCursor(null, screenCenter, CursorMode.Auto);

            rbPlayer.isKinematic = true;
            rbPlayer.position = startingPoint.position;
            rbPlayer.rotation = startingPoint.rotation;
            rbPlayer.isKinematic = false;
        }

        // lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //// storing new direction in the Direction object
        // storing new direction in the Direction object
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        UpdateDirection(mouseX, mouseY);

        //// Move Grabber, needs to be in Update to be updated every frame
        MoveGrabber();

        //// Interact with objects using Raycast
        // Selecting Layer 6: Interactable Objects for our RayCast
        int layerMask = LayerMask.GetMask("Interactable Objects", "Default");

        // we RayCast using the CameraDirection as we don't rotate the Player object, also accounts for pitch
        Ray interactRay = new Ray(tCameraDirection.position, tCameraDirection.TransformDirection(Vector3.forward));
        RaycastHit hit;
        bool hasHit = Physics.Raycast(interactRay, out hit, interactDistance, layerMask, QueryTriggerInteraction.Ignore);
        bool isInteractKeyDown = Input.GetKeyDown(KeyCode.E); // TODO: eventually make it changeable

        if (isInteractKeyDown && hasHit && hit.collider.gameObject.CompareTag("Interactable") && !isInteracting)
        {
            // base interact
            isInteracting = true;
            InteractInformation info = new InteractInformation(tGrabber, hit.transform);
            BaseInteraction(info);
        }
        // If it's not interacting and is holding an item, throw it
        else if (pickedItem != null && !isInteracting)
        {
            isThrowing = (Input.GetKey(KeyCode.E));
            if (isThrowing)
            {
                throwTime = Mathf.Clamp(throwTime + Time.deltaTime, 0f, maxThrowTime);
                throwSlider.GetComponent<ThrowBarManager>().SetValuePercentage(throwTime / maxThrowTime);

            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                isThrowing = false;
                float throwForce = (maxThrowForce * throwTime) / maxThrowTime;
                throwTime = 0f;
                ThrowInformation throwInfo = new ThrowInformation(throwForce, tCameraDirection.forward);
                ThrowInteraction(throwInfo);
                throwSlider.GetComponent<ThrowBarManager>().SetValuePercentage(0f);
            }
        }

        // the isInteracting flag is reset only when the Interaction key is up, to avoid activating multiple interaction types
        if (isInteracting && Input.GetKeyUp(KeyCode.E)) isInteracting = false;
    }

    void FixedUpdate()
    {
        // Player Movement, using raw input to make movement snappier
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        MovePlayer(horizontalMovement, verticalMovement);
    }

    // updates the child object Camera Direction rotation and the player renderer
    private void UpdateDirection(float mouseX, float mouseY)
    {
        pitch -= mouseY * mouseYSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minXRotation, maxXRotation);
        yaw += mouseX * mouseXSensitivity * Time.deltaTime;
        tCameraDirection.rotation = Quaternion.Euler(pitch, yaw, 0);

        //rotate player renderer, not rigidbody to avoid physics issues
        //// this means that the rb never rotates as is, could make issues. In case would need a rb.MoveRotation and interpolation
        tRenderer.eulerAngles = new Vector3(0,tCameraDirection.eulerAngles.y,0);
    }

    // moves the player's rigidbody and updates the Camera Direction position as well, not the rotation
    private void MovePlayer(float horizontalMovement, float verticalMovement)
    {
        Vector3 movement = tCameraDirection.forward * verticalMovement + tCameraDirection.right * horizontalMovement;
        movement.Normalize();
        movement *= movementSpeed * Time.fixedDeltaTime;
        movement.y = rbPlayer.velocity.y; // As we're manipulating speed directly, take care not changing vertical speed
        rbPlayer.AddForce(movement - rbPlayer.velocity,ForceMode.VelocityChange);

        Vector3 newPosition = rbPlayer.position;
        newPosition.y += cameraYOffset;
        tCameraDirection.position = newPosition;
    }

    // updates the grabber position and rotation based on the latest position and rotation of CameraDirection
    private void MoveGrabber()
    {
        // Grabber position is calculated from camera, player's POV
        Vector3 newGrabberPosition = tCameraDirection.forward;
        newGrabberPosition += tCameraDirection.TransformVector(0, grabberYOffset, 0);
        newGrabberPosition *= grabberDistance;
        newGrabberPosition += tCameraDirection.position;

        // Grabber smoothly follow the new position
        smoothedGrabberPosition = Vector3.Lerp(smoothedGrabberPosition, newGrabberPosition, grabberPositionSmoothSpeed * Time.deltaTime);
        smoothedGrabberRotation = Quaternion.Slerp(smoothedGrabberRotation, tCameraDirection.rotation, grabberRotationSmoothSpeed * Time.deltaTime);
        tGrabber.position = smoothedGrabberPosition;
        tGrabber.rotation = smoothedGrabberRotation;
    }

    public void BaseInteraction(InteractInformation interactInfo)
    {
        Interacted?.Invoke(interactInfo);
    }

    public void ThrowInteraction(ThrowInformation throwInfo)
    {
        Thrown?.Invoke(throwInfo);
    }
}
