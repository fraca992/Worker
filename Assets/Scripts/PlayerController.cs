using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using UnityEditor;
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
    private float throwForce = 0f;
    private float throwTime = 0f;
    private float maxThrowTime = 1f; // hardcoding 1s max throw time
    public GameObject pickedItem = null;
    public event Action<Transform> PickedUp;



    [Header("Misc")]
    [SerializeField] private Transform startingPoint;
    [SerializeField] private float cameraYOffset = 0.8f;

    private Rigidbody rbPlayer;
    private Transform tPlayer;
    private Transform tCameraDirection;
    private Transform tGrabber;

    void Start()
    {
        // Get References
        rbPlayer = GetComponent<Rigidbody>();
        tPlayer = GetComponent<Transform>();
        tCameraDirection = tPlayer.Find("CameraDirection");
        tGrabber = tPlayer.Find("Grabber");

        // Apply components properties
        rbPlayer.constraints = RigidbodyConstraints.FreezeRotation;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Spawn on Starting point TODO: not really working as intended. probably best to move to a game manager
        if (startingPoint != null )
        {
            rbPlayer.isKinematic = true;
            rbPlayer.position = startingPoint.position;
            rbPlayer.rotation = startingPoint.rotation;
            rbPlayer.isKinematic = false;
        }
    }

    private void Update()
    {
        // sotring new direction in the Direction object
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        UpdateDirection(mouseX, mouseY);

        // Move Grabber
        MoveGrabber();

        ///// Interact with objects using Raycast
        // pick up obbject
        bool isInteractKeyDown = Input.GetKeyDown(KeyCode.E); // TODO: eventually make it changeable
        bool isInteracting = InteractWithObject(isInteractKeyDown);

        // throw object
        bool isThrowing = (isInteractKeyDown || Input.GetKey(KeyCode.E)) && !isInteracting && pickedItem != null;
        if (isThrowing)
        {
            throwTime = Mathf.Clamp(throwTime + Time.deltaTime, 0f, maxThrowTime);
            Debug.Log("throwing! Time: " + throwTime);
        }
        else
        {
            throwForce = (maxThrowForce * throwTime) / maxThrowTime;
            isThrowing = false;
            // invoking throwed as pickup interaction, then passing Force to PickupObserver
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

        Vector3 newPosition = tPlayer.position;
        newPosition.y += cameraYOffset;
        tCameraDirection.position = newPosition;
    }

    private void MovePlayer(float horizontalMovement, float verticalMovement)
    {
        Vector3 movement = tCameraDirection.forward * verticalMovement + tCameraDirection.right * horizontalMovement;
        movement.Normalize();
        movement *= movementSpeed * Time.fixedDeltaTime;
        movement.y = rbPlayer.velocity.y; // As we're manipulating speed directly, take care not changing vertical speed
        rbPlayer.AddForce(movement - rbPlayer.velocity,ForceMode.VelocityChange);
        rbPlayer.rotation = Quaternion.Euler(0f, tCameraDirection.rotation.y, 0f);
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
            if (hit.collider.gameObject.tag == "Pickup") //TODO: add other interactions with E key here, also is this IF needed?
            {
                PickupInteraction();
            }
            hasInteracted = true;
        }
        return hasInteracted;
    }

    public void PickupInteraction()
    {
        PickedUp?.Invoke(tGrabber);
    }
}
