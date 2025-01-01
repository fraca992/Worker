using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Commons;

public class PickupObserver : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController PlayerSubject;
    [SerializeField] private float maxGrabberDistance = 1f;
    [SerializeField] private float grabberForce = 500f;

    private Rigidbody rb;
    private Transform grabberTransform = null;
    private bool isHeld = false;
    private bool isBeingPickedUp = false;

    void Start()
    {
        // Subscribe to Subject
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted += OnInteract;
            PlayerSubject.Thrown += OnThrow;
        }

        // Get References
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!isHeld && !isBeingPickedUp) return;

        // vector from this item and the player grabber
        Vector3 vectorToGrabber = grabberTransform.position - rb.position;
        Vector3 directionToGrabber = vectorToGrabber.normalized;
        float distanceFromGrabber = vectorToGrabber.magnitude;

        // calculating the total force to move te item towards the grabber as a custom spring/dampener system:
        //// uses an exponential force (the -1 is so that when the distance is 0, force is 0)
        //// combined to a dampener force (in the form of a Velocity change) computed as a decaying exponential
        Vector3 force = grabberForce * (Mathf.Exp(distanceFromGrabber) - 1) * directionToGrabber;
        Vector3 dampeningSpeed = -Mathf.Exp(-distanceFromGrabber) * rb.velocity;
        rb.AddForce(force, ForceMode.Force);
        rb.AddForce(dampeningSpeed, ForceMode.VelocityChange);

        // if the item is too far from the grabber, drop it
        float grabberDistance = (grabberTransform.position - rb.position).magnitude;
        if (!isBeingPickedUp && grabberDistance >= maxGrabberDistance)
        {
            ThrowInformation dropThrow = new ThrowInformation(0f, Vector3.zero);
            OnThrow(dropThrow);
        }
    }

    // triggered when Subject invokes the Interact method
    public void OnInteract(InteractInformation info)
    {
        // returns if it's not interacting with this object, or if it alrady has been pickedup
        if (isHeld || info.interactableTransform != transform) return;

        // getting grabber info and setting interaction flags
        grabberTransform = info.grabberTransform;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = this.gameObject;
        isBeingPickedUp = true;
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // RigidBody settings for pickup
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        StartCoroutine(PickupFlag());
    }

    IEnumerator PickupFlag()
    {
        //yield return new WaitForSeconds(Time.deltaTime);//

        while ((grabberTransform.position - rb.position).magnitude > maxGrabberDistance/2)
        {
            yield return null;
        }

        // resetting interaction flags once it's close enough to not trigger the drop
        isBeingPickedUp = false;
        isHeld = true;
        yield return null;
    }

    // triggered when Subject invokes the Thrown method
    public void OnThrow(ThrowInformation ti)
    {
        // setting interaction flags and Rigidbody settings for throw
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // trhow
        rb.AddForce(ti.force * ti.direction,ForceMode.Impulse);

        // reset interaction flags
        isHeld = false;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = null;
        this.gameObject.layer = LayerMask.NameToLayer("Interactable Objects");
        grabberTransform = null;
    }

    // ignoring collisions with the Player
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }

    // when destroyed, unsubscribe from subject
    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted -= OnInteract;
            PlayerSubject.Thrown -= OnThrow;
        }
    }
}
