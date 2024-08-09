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

        float force;
        Vector3 direction;
    }
}
