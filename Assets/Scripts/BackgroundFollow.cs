using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;
    private float xOffset;
    private float yOffset;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            xOffset = transform.position.x - cameraTransform.position.x;
            yOffset = transform.position.y - cameraTransform.position.y;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 pos = transform.position;
        pos.x = cameraTransform.position.x + xOffset;
        pos.y = cameraTransform.position.y + yOffset;
        transform.position = pos;
    }
}