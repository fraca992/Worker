using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commons : MonoBehaviour
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
}
