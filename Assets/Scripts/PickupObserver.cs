using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObserver : MonoBehaviour
{
    [Header("Observer")]
    [SerializeField] PlayerController subjectToObserve;

    private Rigidbody rb;
    private Transform t;

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

    public void OnInteraction(Transform playerGrabber)
    {
        Debug.Log("picked!");

        t.SetParent(playerGrabber, true);
        rb.MovePosition(Vector3.zero);
        rb.MoveRotation(Quaternion.identity);
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
