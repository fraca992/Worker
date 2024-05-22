using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCameraController : MonoBehaviour
{
    private Camera debugCamera;
    [SerializeField]
    private Camera playerCamera;
    private enum Option
    {
        Disabled,
        SplitScreen,
        OnlyDebugCamera
    }
    [SerializeField]
    private Option DebugOptions;

    

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null) { Debug.LogWarning("Player Camera not assigned!"); return; }
        debugCamera = GetComponent<Camera>(); 

        switch (DebugOptions)
        {
            case Option.Disabled:
                break;
            case Option.SplitScreen:
                playerCamera.rect = new Rect(0, 0, 0.5f, 1);
                debugCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
                break;
            case Option.OnlyDebugCamera:
                playerCamera.enabled = false;
                debugCamera.enabled = true;
                break;
            default:
                break;
        }
    }
}
