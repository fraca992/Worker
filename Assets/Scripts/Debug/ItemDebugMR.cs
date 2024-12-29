using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDebugMR : MonoBehaviour
{
    private Transform playerTransform;
    private Rigidbody playerRigidBody;
    private Transform grabberTransform;
    private Transform t;
    private Rigidbody rb;
    public float positionSmoothSpeed = 10f; // Smooth speed for position
    public float rotationSmoothSpeed = 10f; // Smooth speed for rotation

    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        playerRigidBody = playerTransform.GetComponent<Rigidbody>();
        grabberTransform = playerTransform.GetChild(0).transform;
        t = this.transform;
        rb = this.GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        // Smooth position update
        Vector3 targetPosition = grabberTransform.position;
        Vector3 currentPosition = rb.position;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, positionSmoothSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // Smooth rotation update
        Quaternion targetRotation = grabberTransform.rotation;
        Quaternion currentRotation = rb.rotation;
        Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSmoothSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        // Ensure no unwanted drift
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
