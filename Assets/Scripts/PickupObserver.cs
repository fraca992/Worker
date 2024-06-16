using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;

public class PickupObserver : MonoBehaviour
{
    [Header("Observer")]
    [SerializeField] PlayerController PlayerSubject;

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
            PlayerSubject.PickedUp += OnPickup;
        }
    }

    private void FixedUpdate()
    {
        if (!isPicked) return;

        

        Vector3 movement = grabberTransform.position-t.position;
        rb.velocity = Vector3.zero;
        if (movement.magnitude > 0.01)
        {
            movement.Normalize();
            movement *= (Time.fixedDeltaTime + 100f);
            rb.AddForce(movement, ForceMode.Force);
        }
        
    }

    public void OnPickup(Transform playerGrabber)
    {
        grabberTransform = playerGrabber;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = this.gameObject;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        lerpStart = transform;
        StartCoroutine(PickUpLerp());
        
    }

    IEnumerator PickUpLerp()
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
        //t.SetParent(grabberTransform);
        rb.isKinematic = false;
        rb.freezeRotation = true;

        yield return null;
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
            PlayerSubject.PickedUp -= OnPickup;
        }
    }
}
