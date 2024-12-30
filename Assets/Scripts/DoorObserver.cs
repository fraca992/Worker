using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using static Commons;

public class DoorObserver : MonoBehaviour, IInteractable
{
    [SerializeField] PlayerController PlayerSubject;
    [SerializeField] float openingTime = 1f;
    [SerializeField] float openingAngle = 95f;

    Transform grabberTransform;
    Vector3 closedRotation;
    bool hasActivated = false;
    bool isOpen = false;

    void Start()
    {
        // Subscribe to Subject
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted += OnInteract;
        }

        closedRotation = transform.rotation.eulerAngles;
        // ensuring the Renderer & Collider (Child 0) is in the correct offset
        Transform rendererTransform = transform.GetChild(0);
        rendererTransform.localPosition = new Vector3(rendererTransform.GetComponent<MeshRenderer>().bounds.size.x/2, 0f, 0f);
    }

    public void OnInteract(InteractInformation info)
    {
        if (hasActivated || info.interactableTransform.parent != transform) return;
        
        // getting grabber info and setting interaction flags
        grabberTransform = info.grabberTransform;
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = true;
        hasActivated = true;

        // setting the target location depending on isOpen and player position, then start open door coroutine
        Vector3 targetRotation;
        float playerDoorDot = Vector3.Dot((transform.position - grabberTransform.parent.position).normalized, transform.forward);

        if (isOpen)
        {
            targetRotation = closedRotation;
        }
        else if (playerDoorDot > 0f)
        {
            targetRotation = new Vector3(transform.rotation.x, transform.rotation.y - openingAngle, transform.rotation.z);
        }
        else
        {
            targetRotation = new Vector3(transform.rotation.x, transform.rotation.y + openingAngle, transform.rotation.z);
        }

        isOpen = !isOpen;
        Quaternion lerpStart = transform.rotation;
        StartCoroutine(OpenDoor(targetRotation, lerpStart));
    }

    // Coroutine which opens the door using a Slerp interpolation over openingTime
    IEnumerator OpenDoor(Vector3 targetRotation, Quaternion lerpStart)
    {
        float timeElapsed = 0;

        while (timeElapsed < openingTime)
        {
            Quaternion interpolatedRotation = Quaternion.Slerp(lerpStart, Quaternion.Euler(targetRotation), timeElapsed / openingTime);
            transform.rotation = interpolatedRotation;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.Euler(targetRotation);

        // reset flags
        grabberTransform.GetComponentInParent<PlayerController>().isInteracting = false;
        hasActivated = false;
        yield return null;
    }

    // when destroyed, unsubscribe from subject
    private void OnDestroy()
    {
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted -= OnInteract;
        }
    }
}
