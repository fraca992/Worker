using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;


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
    private FixedJoint joint;

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



        //Vector3 movement = grabberTransform.position-t.position;
        //movement *= (Time.fixedDeltaTime + 20f);
        //rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
        //rb.AddForce(movement, ForceMode.VelocityChange);
        joint.connectedAnchor = grabberTransform.localPosition;
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
        rb.isKinematic = false;
        rb.freezeRotation = true;

        joint = this.AddComponent<FixedJoint>();
        joint.anchor = Vector3.zero;
        joint.connectedBody = grabberTransform.GetComponentInParent<Rigidbody>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grabberTransform.localPosition;

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
