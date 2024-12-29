using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 yOffset = new Vector3(0, 0.5f, 0);
        Debug.DrawLine(transform.position + yOffset
            , transform.position + transform.TransformDirection(transform.forward) + yOffset, Color.red, Time.deltaTime);
    }
}