using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using UnityEditor;
using System.Runtime.CompilerServices;
using static Commons;
using UnityEngine.UI;
//using UnityEngine.Windows;

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
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float grabberDistance = 1.5f;
    [SerializeField] private float grabberYOffset = -0.11f;
    [SerializeField] private float maxThrowForce = 10f;
    private float throwTime = 0f;
    private float maxThrowTime = 2f; // hardcoding 1s max throw time
    public GameObject pickedItem = null;
    public event Action<InteractInformation> Interacted;
    public event Action<ThrowInformation> Thrown;
    [SerializeField] private Slider throwSlider;
    public bool isInteracting = false;

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

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // storing new direction in the Direction object
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        UpdateDirection(mouseX, mouseY);

        // Move Grabber
        MoveGrabber();

        ///// Interact with objects using Raycast
        // pick up obbject
        bool isInteractKeyDown = Input.GetKeyDown(KeyCode.E); // TODO: eventually make it changeable
        InteractWithObject(isInteractKeyDown && !isInteracting);

        // throw object
        if (pickedItem != null && !isInteracting)
        {
            bool isThrowing = (Input.GetKey(KeyCode.E));
            if (isThrowing)
            {
                throwTime = Mathf.Clamp(throwTime + Time.deltaTime, 0f, maxThrowTime);
                throwSlider.GetComponent<ThrowBarManager>().SetValuePercentage(throwTime/maxThrowTime);

            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                float throwForce = (maxThrowForce * throwTime) / maxThrowTime;
                throwTime = 0f;
                isThrowing = false;
                ThrowInformation throwInfo = new ThrowInformation(throwForce, tCameraDirection.forward);
                ThrowInteraction(throwInfo);
                throwSlider.GetComponent<ThrowBarManager>().SetValuePercentage(0f);
            }
        }
    }

    void FixedUpdate()
    {
        // Player Movement
        float horizontalMovement = Input.GetAxisRaw("Horizontal"); // using raw input to make movement snappier
        float verticalMovement = Input.GetAxisRaw("Vertical");
        MovePlayer(horizontalMovement, verticalMovement);
    }

    private void UpdateDirection(float mouseX, float mouseY)
    {
        pitch -= mouseY * mouseYSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minXRotation, maxXRotation);
        yaw += mouseX * mouseXSensitivity * Time.deltaTime;
        tCameraDirection.rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 newPosition = rbPlayer.position; // using player RigidBody results in jittery camera, so I'm using Transform
        newPosition.y += cameraYOffset;
        tCameraDirection.position = newPosition;

        //rotate player renderer, not rigidbody to avoid physics issues
        tRenderer.eulerAngles = new Vector3(0,tCameraDirection.eulerAngles.y,0);
    }

    private void MovePlayer(float horizontalMovement, float verticalMovement)
    {
        Vector3 movement = tCameraDirection.forward * verticalMovement + tCameraDirection.right * horizontalMovement;
        movement.Normalize();
        movement *= movementSpeed * Time.fixedDeltaTime;
        movement.y = rbPlayer.velocity.y; // As we're manipulating speed directly, take care not changing vertical speed
        rbPlayer.AddForce(movement - rbPlayer.velocity,ForceMode.VelocityChange);
    }

    private void MoveGrabber()
    {
        // Grabber position is calculated from camera, player's POV
        Vector3 newGrabberPosition = tCameraDirection.forward;
        newGrabberPosition += tCameraDirection.TransformVector(0, grabberYOffset, 0);
        newGrabberPosition *= grabberDistance;
        tGrabber.position = tCameraDirection.position + newGrabberPosition;
        tGrabber.rotation = tCameraDirection.rotation;
    }

    private bool InteractWithObject(bool interactKey)
    {
        //Selecting Layer 6: Interactable Obects for our RayCast
        int layerMask = 1 << 6;

        //we RayCast using the Camera rather than the player to account for pitch
        Ray interactRay = new Ray(tCameraDirection.position, tCameraDirection.TransformDirection(Vector3.forward));
        RaycastHit hit;
        bool hasInteracted = false;
        if (interactKey && Physics.Raycast(interactRay, out hit, interactDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (pickedItem == null && hit.collider.gameObject.tag == "Interactable")
            {
                InteractInformation info = new InteractInformation(tGrabber,hit.transform);
                BaseInteraction(info);
            }

            hasInteracted = true;
        }
        return hasInteracted;
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
