using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform tPlayer;
    private Transform tPlayerDirection;

    void Start()
    {
        if (tPlayer.Find("CameraDirection").transform == null) { Debug.LogWarning("Could not find Player Direction!"); return; }
        tPlayerDirection = tPlayer.Find("CameraDirection").transform;
    }

    void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 newPosition = tPlayerDirection.position;
        transform.position = newPosition;
        transform.rotation = tPlayerDirection.rotation;
    }
}