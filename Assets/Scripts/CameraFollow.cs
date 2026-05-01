using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;
    public float verticalOffset = 2f;
    public float minYLimit = -8f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        float targetY = target.position.y + verticalOffset;
        targetY = Mathf.Max(targetY, minYLimit);

        Vector3 desiredPosition = new Vector3(0f, targetY, transform.position.z);

        // Smooth follow WITHOUT jitter
        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            0.1f
        );

        // Add shake ON TOP (not inside smoothing)
        if (CameraShake.Instance != null)
        {
            smoothPosition += CameraShake.Instance.GetShakeOffset();
        }

        transform.position = smoothPosition;
    }
}