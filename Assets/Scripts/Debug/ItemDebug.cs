using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDebug : MonoBehaviour
{
    private Transform playerTransform;
    private Rigidbody playerRigidBody;
    private Transform t;
    private Rigidbody rb;
    private ConfigurableJoint joint;

    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        playerRigidBody = playerTransform.GetComponent<Rigidbody>();
        t = this.transform;
        rb = this.GetComponent<Rigidbody>();
        joint = this.GetComponent<ConfigurableJoint>();
        joint.connectedBody = playerRigidBody;
        joint.enableCollision = false;
        joint.connectedAnchor = Vector3.zero;

        JointDrive jd_linear = new JointDrive
        {
            maximumForce = Mathf.Infinity,
            positionSpring = 500f,
            positionDamper = 2 * Mathf.Sqrt(500f * rb.mass) //critical linear dampening
        };

        // Approximate moment of inertia for a box-shaped Rigidbody
        Vector3 size = rb.GetComponent<Collider>().bounds.size;
        float mass = rb.mass;
        float approxInertia = (1f / 12f) * mass * (size.x * size.x + size.y * size.y + size.z * size.z);
        float damping = 2 * Mathf.Sqrt(500f * approxInertia); // critical angular dampening, relies on Inertia

        JointDrive jd_angular = new JointDrive
        {
            maximumForce = Mathf.Infinity,
            positionSpring = 500f,
            positionDamper = damping
        };

        joint.xDrive = jd_linear;
        joint.yDrive = jd_linear;
        joint.zDrive = jd_linear;

        joint.angularXDrive = jd_angular;
        joint.angularYZDrive = jd_angular;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.linearLimit = new SoftJointLimit
        {
            limit = 10,
            bounciness = 0,
            contactDistance = 1
        };
    }

    void FixedUpdate()
    {
        joint.targetPosition = new Vector3(-playerTransform.GetChild(0).localPosition.x,
            -playerTransform.GetChild(0).localPosition.y,
            -playerTransform.GetChild(0).localPosition.z); //need minus as constraint is coming from item and not player
        //joint.targetVelocity = t.InverseTransformPoint(playerTransform.GetChild(0).position);
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.IsPlaying(this)) return;
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(playerTransform.position + joint.targetPosition, 0.2f);
    }
}
