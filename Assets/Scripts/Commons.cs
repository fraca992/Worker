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

    public struct InteractInformation
    {
        public InteractInformation(Transform g, Transform i)
        {
            grabberTransform = g;
            interactableTransform = i;
        }

        public Transform grabberTransform;
        public Transform interactableTransform;
    }

    public interface IInteractable
    {
        public void OnInteract(InteractInformation info);
    }
}
