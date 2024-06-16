using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using UnityEditor;

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
    public GameObject pickedItem = null;
    public event Action<Transform> PickedUp;

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

        // Apply components properties
        rbPlayer.constraints = RigidbodyConstraints.FreezeRotation;

        // Spawn on Starting point
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

        // Interact with objects using Raycast
        bool interactKey = Input.GetKeyDown(KeyCode.E);
        InteractWithObject(interactKey);

        // Move Grabber
        MoveGrabber();
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

        //rotate player rendere, not rigidbody
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

    private void InteractWithObject(bool interactKey)
    {
        //Selecting Layer 6: Interactable Obects for our RayCast
        int layerMask = 1 << 6;

        //we RayCast using the Camera rather than the player to account for pitch
        Ray interactRay = new Ray(tCameraDirection.position, tCameraDirection.TransformDirection(Vector3.forward));
        RaycastHit hit;
        if (interactKey && Physics.Raycast(interactRay, out hit, interactDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Pickup") //TODO: add other interactions with E key here, also is this IF needed?
            {
                PickupInteraction();
            }
        }
    }

    public void PickupInteraction()
    {
        PickedUp?.Invoke(tGrabber);
    }
}
