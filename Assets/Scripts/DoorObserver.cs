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
            PlayerSubject.Interacted += OnInteract; // TODO: should be its own interact
        }

        
    }

    void FixedUpdate()
    {
        // rotate Door if it was activated
        if (hasActivated && rb.rotation.eulerAngles.y < 95 )
        {
            Quaternion deltaRotation = Quaternion.Euler(doorRotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        else if (hasActivated && !(rb.rotation.eulerAngles.y < 95))
        {
            // once the door has finished opening, clear activated flag and freeze all
            rb.constraints = RigidbodyConstraints.FreezeAll;
            hasActivated = false;
            grabberTransform.GetComponentInParent<PlayerController>().isInteracting = false;
        }

        //TODO: this only works opening the door! should also close. also, open away from player
    }

    // When interacted, the Door object will prepare for the rotation implemented in FixedUpdate()
    public void OnInteract(InteractInformation info)
    {
        if (hasActivated || info.interactableTransform != transform) return;

        grabberTransform = info.grabberTransform;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = true;

        hasActivated = true;

        // clear constraints, then freeze all rotation except on Y axis (hinges)
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        rb.constraints = RigidbodyConstraints.FreezeRotationX;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted -= OnInteract;
        }
    }
}
