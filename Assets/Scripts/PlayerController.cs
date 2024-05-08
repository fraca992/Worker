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
    private float mouseSensitivity = 500f;
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

    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private Transform cameraTransform;
    private Transform grabberTransform;

    void Start()
    {
        // Get References
        playerRigidbody = GetComponent<Rigidbody>();
        playerTransform = GetComponent<Transform>();
        cameraTransform = playerTransform.GetChild(0);
        grabberTransform = playerTransform.GetChild(1);

        // Apply components properties
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Spawn on Starting point
        if (startingPoint != null )
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.position = startingPoint.position;
            playerRigidbody.rotation = startingPoint.rotation;
            playerRigidbody.isKinematic = false;
        }
    }


    
    private void Update()
    {
        // Player & Camera rotation
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        LookAndRotate(mouseX, mouseY);

        // Interact with objects using Raycast
        bool interactKey = Input.GetKeyDown(KeyCode.E);
        InteractWithObject(interactKey); //TODO: rewrite? put if outside of function for clarity

        // Move Grabber
        Vector3 newGrabberPosition = cameraTransform.TransformDirection(Vector3.forward + new Vector3(0, grabberYOffset, 0));
        newGrabberPosition *= grabberDistance;
        newGrabberPosition += cameraTransform.position;
        grabberTransform.position = newGrabberPosition;
        grabberTransform.rotation = cameraTransform.rotation;
    }

    void FixedUpdate()
    {
        // Player Movement
        float horizontalMovement = Input.GetAxisRaw("Horizontal"); // using raw input to make movement snappier
        float verticalMovement = Input.GetAxisRaw("Vertical");
        MovePlayer(horizontalMovement, verticalMovement);
    }

    private void LookAndRotate(float mouseX, float mouseY)
    {
        pitch -= mouseY * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minXRotation, maxXRotation);
        yaw += mouseX * mouseSensitivity * Time.deltaTime;

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0); 
        playerTransform.localRotation = Quaternion.Euler(0, yaw, 0); //FIX: this rotation stutters when moving: needs to be interpolated
    }

    private void MovePlayer(float horizontalMovement, float verticalMovement)
    {
        Vector3 movement = transform.forward * verticalMovement + transform.right * horizontalMovement;
        movement.Normalize();
        movement *= movementSpeed * Time.fixedDeltaTime;
        movement.y = playerRigidbody.velocity.y; // As we're manipulating speed directly, take care not changing vertical speed
        playerRigidbody.velocity = movement; // this could create weird physics interactions. If so, maybe try AddForce with ForceMode.VelocityChange, clamp to max velocity
    }

    private void InteractWithObject(bool interactKey)
    {
        //Selecting Layer 6: Interactable Obects for our RayCast
        int layerMask = 1 << 6;

        //we RayCast using the Camera rather than the player to account for pitch
        Ray interactRay = new Ray(cameraTransform.position, cameraTransform.TransformDirection(Vector3.forward));
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
            Interacted?.Invoke(grabberTransform);
    }
}
