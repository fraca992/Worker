using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Commons;

public class PickupObserver : MonoBehaviour, IInteractable
{
    [Header("Observer")]
    [SerializeField] PlayerController PlayerSubject;
    [SerializeField] float maxGrabberDistance = 1f;

    private Rigidbody rb;
    private Transform t;
    private Transform grabberTransform = null;
    private bool isPicked = false;


    #region Pickup Lerp properties
    float lerpDuration = 0.5f;
    Transform lerpStart;
    Vector3 interpolatedPosition;
    Vector3 interpolatedRotation;
    #endregion


    void Start()
    {
        // Get References
        rb = GetComponent<Rigidbody>();
        t = GetComponent<Transform>();

        // Subscribe to Subject
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted += OnInteract;
            PlayerSubject.Thrown += OnThrow;
        }
    }

    private void FixedUpdate()
    {
        if (!isPicked) return;

        Vector3 endPoint = grabberTransform.position;
        Vector3 startPoint = rb.position; // or transform?
        Vector3 cursorVector = endPoint - startPoint;
        Vector3 cursorVectorDirection = cursorVector.normalized;
        float cursorVectorMagnitude = cursorVector.magnitude;

        float forceMagnitude = 500f;
        Vector3 force = forceMagnitude * (Mathf.Exp(cursorVectorMagnitude) - 1) * cursorVectorDirection;
        float dampenMagnitude = rb.mass * (Mathf.Pow(rb.velocity.magnitude, 2) / (2 * 0.001f));
        Vector3 dampenerForce = dampenMagnitude * Mathf.Exp(-cursorVectorMagnitude) * -cursorVectorDirection;

        rb.AddForce(force, ForceMode.Force);
        rb.AddForce(-Mathf.Exp(-cursorVectorMagnitude) * rb.velocity, ForceMode.VelocityChange);

        float grabberDistance = (grabberTransform.position - rb.position).magnitude;
        if (grabberDistance >= maxGrabberDistance)
        {
            ThrowInformation dropThrow = new ThrowInformation(0f, Vector3.zero);
            OnThrow(dropThrow);
        }

    }

    public void OnInteract(InteractInformation info)
    {
        if (isPicked || info.interactableTransform != transform) return;

        grabberTransform = info.grabberTransform;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = this.gameObject;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        lerpStart = transform;
        StartCoroutine(PickUpLerp());
        
    }
    IEnumerator PickUpLerp() // CONSIDER USING MOVEPOSITION INSTEAD OF COROUTINE
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            interpolatedPosition = Vector3.Lerp(lerpStart.position, grabberTransform.position, timeElapsed / lerpDuration);
            interpolatedRotation = Vector3.Lerp(lerpStart.rotation.eulerAngles, grabberTransform.rotation.eulerAngles, timeElapsed / lerpDuration);
            t.position = interpolatedPosition;
            t.rotation = Quaternion.Euler(interpolatedRotation);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        t.position = grabberTransform.position;
        t.rotation = grabberTransform.rotation;
        isPicked = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = false;

        yield return null;
    }

    public void OnThrow(ThrowInformation ti)
    {
        //TODO: Implement throw logic
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = true;
        rb.isKinematic = false;
        rb.freezeRotation = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        Debug.DrawLine(grabberTransform.position, grabberTransform.position + (ti.direction * ti.force), Color.red, 5f);
        rb.AddForce(ti.force * ti.direction,ForceMode.Impulse);

        isPicked = false;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = null;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = false;
        grabberTransform = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }

    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted -= OnInteract;
            PlayerSubject.Thrown -= OnThrow;
        }
    }
}
