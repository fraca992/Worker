using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Commons;

public class DoorObserver : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController PlayerSubject;
    [SerializeField] private float openingTime = 0.5f;
    [SerializeField] private float openingAngle = 95f;

    private Transform grabberTransform;
    private Vector3 closedRotation;
    private bool hasActivated = false;
    private bool isOpen = false;

    void Start()
    {
        // Subscribe to Subject
        if (PlayerSubject != null)
        {
            PlayerSubject.Interacted += OnInteract;
        }

        // Get references
        Transform rendererTransform = transform.GetChild(0);

        // Initializing values
        closedRotation = transform.rotation.eulerAngles;
        rendererTransform.localPosition = new Vector3(rendererTransform.GetComponent<MeshRenderer>().bounds.size.x/2, 0f, 0f);
    }

    // triggered when Subject invokes the Interacted method
    public void OnInteract(InteractInformation info)
    {
        // returns if it's not interacting with this object, or if it alrady has activated
        if (hasActivated || info.interactableTransform.parent != transform) return;
        
        // getting grabber info and setting interaction flags
        grabberTransform = info.grabberTransform;
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

        // reset interaction flags
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
