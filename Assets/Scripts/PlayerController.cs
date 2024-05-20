using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
//using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float movementSpeed = 250f;
    [SerializeField]
    private float mouseXSensitivity = 500f;
    [SerializeField]
    private float mouseYSensitivity = 500f;
    [SerializeField]
    private float cameraYOffset = 0.8f;
    [SerializeField]
    private int minXRotation = -60;
    [SerializeField]
    private int maxXRotation = +60;
    private float pitch = 0;
    private float yaw = 0;

    [Header("Interact")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float grabberDistance = 1.5f;
    [SerializeField] private float grabberYOffset = -0.3f;
    public GameObject pickedItem = null;
    public event Action<Transform> Interacted;

    [Header("Misc")]
    [SerializeField] private Transform startingPoint;

    private Rigidbody rbPlayer;
    private Transform tPlayer;
    private Transform tCamera;
    private Transform tDirection;
    private Transform tGrabber;

    void Start()
    {
        // Get References
        rbPlayer = GetComponent<Rigidbody>();
        tPlayer = GetComponent<Transform>();
        tCamera = tPlayer.Find("Camera");
        tDirection = tPlayer.Find("Direction");
        tGrabber = tPlayer.Find("Grabber");

        // Apply components properties
        rbPlayer.constraints = RigidbodyConstraints.FreezeRotation;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Spawn on Starting point
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
        // Player & Camera rotation
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        LookDirection(mouseX, mouseY);

        // Interact with objects using Raycast
        bool interactKey = Input.GetKeyDown(KeyCode.E);
        InteractWithObject(interactKey); //TODO: rewrite? put if outside of function for clarity

        // Move Grabber
        Vector3 newGrabberPosition = tDirection.TransformDirection(Vector3.forward + new Vector3(0, grabberYOffset, 0));
        newGrabberPosition.Normalize();
        newGrabberPosition *= grabberDistance;
        tGrabber.localPosition = newGrabberPosition;
        tGrabber.rotation = tDirection.rotation;
    }

    void FixedUpdate()
    {
        // Player Movement
        float horizontalMovement = Input.GetAxisRaw("Horizontal"); // using raw input to make movement snappier
        float verticalMovement = Input.GetAxisRaw("Vertical");
        //float mouseX = Input.GetAxisRaw("Mouse X");
        MovePlayer(horizontalMovement, verticalMovement);
    }

    private void LateUpdate()
    {
        Vector3 newPosition = new Vector3(tPlayer.position.x, tPlayer.position.y, tPlayer.position.z);
        newPosition.y += cameraYOffset;
        tCamera.position = newPosition;
        tCamera.rotation = tDirection.rotation;
    }

    private void LookDirection(float mouseX, float mouseY)
    {
        pitch -= mouseY * mouseYSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minXRotation, maxXRotation);
        yaw += mouseX * mouseXSensitivity * Time.deltaTime;

        tDirection.rotation = Quaternion.Euler(pitch, yaw, 0); //2 test: Time.DeltaTime & Rotate
        //tPlayer.rotation = Quaternion.Euler(0, yaw, 0);
    }

    private void MovePlayer(float horizontalMovement, float verticalMovement)
    {
        Vector3 movement = tDirection.forward * verticalMovement + tDirection.right * horizontalMovement;
        movement.Normalize();
        movement *= movementSpeed * Time.fixedDeltaTime;
        movement.y = rbPlayer.velocity.y; // As we're manipulating speed directly, take care not changing vertical speed
        rbPlayer.velocity = movement; // this could create weird physics interactions. If so, maybe try AddForce with ForceMode.VelocityChange, clamp to max velocity
        rbPlayer.rotation = Quaternion.Euler(0f, tDirection.rotation.y, 0f);
        //rbPlayer.AddForce(movement, ForceMode.Force);
    }

    private void InteractWithObject(bool interactKey)
    {
        //Selecting Layer 6: Interactable Obects for our RayCast
        int layerMask = 1 << 6;

        //we RayCast using the Direction object rather than the player to account for pitch
        Ray interactRay = new Ray(tDirection.position, tDirection.TransformDirection(Vector3.forward));
        RaycastHit hit;
        if (interactKey && Physics.Raycast(interactRay, out hit, interactDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Pickup") //TODO: add other interactions with E key here, also is this IF needed?
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
            Interacted?.Invoke(tGrabber);
    }
}
