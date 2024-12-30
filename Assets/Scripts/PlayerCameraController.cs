using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform tPlayer;
    [Header("Camera Movement")]
    [SerializeField] private float positionSmoothSpeed = 15f;
    [SerializeField] private float rotationSmoothSpeed = 10f;
    
    private Transform tCameraDirection;
    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;

    void Start()
    {
        if (tPlayer.Find("CameraDirection").transform == null) { Debug.LogWarning("Could not find Player Direction!"); return; }
        tCameraDirection = tPlayer.Find("CameraDirection").transform;

        smoothedPosition = tCameraDirection.position;
        smoothedRotation = tCameraDirection.rotation;
    }

    void LateUpdate()
    {
        // Smoothly follow the camera direction
        smoothedPosition = Vector3.Lerp(smoothedPosition, tCameraDirection.position, positionSmoothSpeed * Time.deltaTime);
        smoothedRotation = Quaternion.Slerp(smoothedRotation, tCameraDirection.rotation, rotationSmoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
        transform.rotation = smoothedRotation;
    }
}