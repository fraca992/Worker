using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Commons;

public class DoorObserver : MonoBehaviour, IInteractable
{
    [Header("Observer")]
    [SerializeField] PlayerController PlayerSubject;
    [SerializeField] float OpeningSpeed = 50f;


    Rigidbody rb;
    Transform grabberTransform;

    bool hasActivated = false;
    private Vector3 doorRotationSpeed = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        doorRotationSpeed.y = OpeningSpeed;

        // Subscribe to Subject
        if (PlayerSubject != null)
        {
            PlayerSubject.PickedUp += OnInteract; // TODO: should be its own interact
        }

        
    }

    void FixedUpdate()
    {
        if (hasActivated && rb.rotation.eulerAngles.y < 95 )
        {
            Quaternion deltaRotation = Quaternion.Euler(doorRotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
            
        }
    }

    public void OnInteract(Transform playerGrabber)
    {
        grabberTransform = playerGrabber;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = true;

        hasActivated = true;

        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.PickedUp -= OnInteract;
        }
    }
}
