using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDebugSpring : MonoBehaviour
{
    private Transform playerTransform;
    private Rigidbody playerRigidBody;
    private Transform grabberTransform;
    private Transform t;
    private Rigidbody rb;
    private SpringJoint joint;

    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        playerRigidBody = playerTransform.GetComponent<Rigidbody>();
        grabberTransform = playerTransform.GetChild(0).transform;
        t = this.transform;
        rb = this.GetComponent<Rigidbody>();
        joint = this.GetComponent<SpringJoint>();
        joint.connectedBody = playerRigidBody;
        joint.enableCollision = true;
        joint.connectedAnchor = grabberTransform.localPosition;
    }

    void FixedUpdate()
    {
        joint.connectedAnchor = grabberTransform.localPosition;
    }
}
