using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform tPlayer;
    private Transform tCameraDirection;

    void Start()
    {
        if (tPlayer.Find("CameraDirection").transform == null) { Debug.LogWarning("Could not find Player Direction!"); return; }
        tCameraDirection = tPlayer.Find("CameraDirection").transform;
    }

    void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 newPosition = tCameraDirection.position;
        transform.position = newPosition;
        transform.rotation = tCameraDirection.rotation;
    }
}