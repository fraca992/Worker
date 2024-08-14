using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Commons
{
    public struct ThrowInformation
    {
        public ThrowInformation(float f, Vector3 dir)
        {
            force = f;
            direction = dir;
        }

        public float force;
        public Vector3 direction;
    }

    public interface IInteractable
    {
        public void OnInteract(Transform playerGrabber);
    }
}
