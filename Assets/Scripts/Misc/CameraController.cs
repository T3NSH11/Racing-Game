using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;
    private Rigidbody playerRB;
    public Vector3 Offset;
    public Vector3 Offset2;
    public float cameraSpeed;
    public Vector3 cameraVelocity;
    public float smoothDelay;

    void Start()
    {
        playerRB = playerTransform.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        transform.LookAt(playerTransform.position + Offset2);
        Vector3 targetPosition = playerTransform.position + playerTransform.transform.TransformVector(Offset) + playerTransform.forward * (-5f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref cameraVelocity, smoothDelay);
    }
}
