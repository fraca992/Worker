using System.Collections;
using System.Collections.Generic;
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
    float lerpDuration = 1;
    Transform lerpStart;
    Transform lerpEnd;
    Vector3 interpolatedPosition;
    Vector3 interpolatedRotation;
    #endregion

    // Start is called before the first frame update
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

    //private void FixedUpdate()
    //{
    //    if (grabberTransform == null) return;

    //    rb.MovePosition(grabberTransform.position);
    //    rb.MoveRotation(grabberTransform.rotation);
    //}
    private void Update()
    {
        if (!isPicked) return;

        t.position = (grabberTransform.position);
        t.rotation = (grabberTransform.rotation);
    }

    public void OnPickup(Transform playerGrabber)
    {
        grabberTransform = playerGrabber;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = this.gameObject;
        rb.useGravity = false;
        rb.isKinematic = true;


        lerpStart = transform;
        lerpEnd = playerGrabber.transform;
        StartCoroutine(PickUpLerp());
        
    }

    IEnumerator PickUpLerp()
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            interpolatedPosition = Vector3.Lerp(lerpStart.position, lerpEnd.position, timeElapsed / lerpDuration);
            interpolatedRotation = Vector3.Lerp(lerpStart.rotation.eulerAngles, lerpEnd.rotation.eulerAngles, timeElapsed / lerpDuration);
            rb.MovePosition(interpolatedPosition);
            rb.MoveRotation(Quaternion.Euler(interpolatedRotation));

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(lerpEnd.position);
        rb.MoveRotation(lerpEnd.rotation);
        //rb.isKinematic = false; // re-enable when you deprecate using position transform to move
        isPicked = true;
    }

    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.PickedUp -= OnPickup;
        }
    }
}
