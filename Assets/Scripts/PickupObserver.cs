using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObserver : MonoBehaviour
{
    [Header("Observer")]
    [SerializeField] PlayerController subjectToObserve;

    private Rigidbody rb;
    private Transform t;
    private Transform grabberTransform = null;

    // Start is called before the first frame update
    void Start()
    {
        // Get References
        rb = GetComponent<Rigidbody>();
        t = GetComponent<Transform>();

        // Subscribe to Subject
        if (subjectToObserve != null)
        {
            subjectToObserve.Interacted += OnInteraction;
        }
    }

    private void FixedUpdate()
    {
        if (grabberTransform == null) return;

        rb.MovePosition(grabberTransform.position);
        rb.MoveRotation(grabberTransform.rotation);
    }

    public void OnInteraction(Transform playerGrabber)
    {
        grabberTransform = playerGrabber;
        grabberTransform.GetComponentInParent<PlayerController>().pickedItem = this.gameObject;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void OnDestroy()
    {
        if (subjectToObserve != null)
        {
            subjectToObserve.Interacted -= OnInteraction;
        }
    }
}
